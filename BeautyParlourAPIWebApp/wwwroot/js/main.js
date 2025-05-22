document.addEventListener('DOMContentLoaded', async () => {
    const boardsContainer = document.getElementById('boards-container');
    const boardTemplate = document.getElementById('board-template');
    const itemTemplate = document.getElementById('portfolio-item-template');
    const searchInput = document.getElementById('search-input');
    const hashtagSuggestionsUl = document.getElementById('hashtag-suggestions');

    console.log('Board template element:', boardTemplate);
    console.log('Item template element:', itemTemplate);

    // Helper to fade color
    function fadeColor(hex, alpha = 0.15) {
        if (!hex) return `rgba(255,255,255,${alpha})`;
        let c = hex.replace('#', '');
        if (c.length === 3) c = c.split('').map(x => x + x).join('');
        const num = parseInt(c, 16);
        const r = (num >> 16) & 255;
        const g = (num >> 8) & 255;
        const b = num & 255;
        return `rgba(${r},${g},${b},${alpha})`;
    }

    // Function to filter boards based on selected hashtag
    async function filterBoards(hashtag = '') {
        const res = await fetch('/api/boards'); // Fetch all boards
        if (!res.ok) return;
        const allBoards = await res.json();

        let filteredBoards = allBoards; // Start with all boards

        if (hashtag) {
            // If a hashtag is selected, filter boards client-side
            filteredBoards = allBoards.filter(board => {
                // Check if this board contains any item with the selected hashtag
                return board.items && board.items.some(item =>
                    item.portfolioItemHashtags && item.portfolioItemHashtags.some(h =>
                        `#${h.hashtag.tag}`.toLowerCase() === hashtag.toLowerCase()
                    )
                );
            });
        }

        boardsContainer.innerHTML = ''; // Clear existing boards
        renderBoards(filteredBoards, hashtag); // Render the filtered list of boards and highlight items
    }

    // Modify loadBoards to call filterBoards initially
    async function loadBoards() {
        filterBoards(); // Load all boards initially
    }

    // New function to render boards (extracted from original loadBoards logic)
    function renderBoards(boards, selectedHashtag = '') {
         for (const board of boards) {
            // No need for boardContainsSelectedHashtagItem flag here anymore,
            // as we are now filtering the boards list itself before rendering.

            const boardNode = boardTemplate.content.cloneNode(true);
            const cardHeader = boardNode.querySelector('.card-header');
            const boardName = boardNode.querySelector('.board-name');
            const boardTitleImage = boardNode.querySelector('.board-title-image');
            const seeMoreBtn = boardNode.querySelector('.see-more-btn');
            const itemsContainer = boardNode.querySelector('.board-items-container');
            const boardCard = boardNode.querySelector('.card'); // Get the card element

            // Set board name
            boardName.textContent = board.name;

            // Set theme color on the card header
            if (board.themeColor) {
                cardHeader.style.setProperty('--theme-color', fadeColor(board.themeColor, 0.25));
            }

            // Handle initial display based on title image
            if (board.titleImageUrl) {
                boardTitleImage.src = board.titleImageUrl;
                boardTitleImage.style.display = ''; // Show title image
                seeMoreBtn.style.display = ''; // Show See more button
                itemsContainer.style.display = 'none'; // Hide items initially
            } else {
                // If no title image or if filtered by hashtag, show items by default
                 boardTitleImage.style.display = 'none'; // Hide title image
                seeMoreBtn.style.display = 'none'; // Hide See more button
                itemsContainer.style.display = ''; // Show items
            }

            // Add event listener to See more button
            seeMoreBtn.addEventListener('click', () => {
                boardTitleImage.style.display = 'none';
                seeMoreBtn.style.display = 'none'; // Hide button after click
                itemsContainer.style.display = '';
            });

             // Add hover effect to show See more button (if title image exists and items are not visible)
            if (board.titleImageUrl) {
                boardCard.addEventListener('mouseenter', () => {
                    if (itemsContainer.style.display === 'none') { // Only show on hover if items are hidden
                         seeMoreBtn.style.display = '';
                    }
                });
                boardCard.addEventListener('mouseleave', () => {
                    // Only hide if the items container is not visible (meaning we are still in the title image view)
                    if (itemsContainer.style.display === 'none') {
                         seeMoreBtn.style.display = 'none';
                    }
                });
            }

            // Render items
            if (board.items && board.items.length) {
                console.log(`Rendering ${board.items.length} items for board ${board.name}...`);
                for (const item of board.items) {
                    console.log('Processing item:', item);
                    const itemNode = itemTemplate.content.cloneNode(true);
                    const beforeImg = itemNode.querySelector('.before-photo');
                    const afterImg = itemNode.querySelector('.after-photo');
                    const hashtagsSpan = itemNode.querySelector('.hashtags');
                    const priceSpan = itemNode.querySelector('.price');
                    const portfolioItemDiv = itemNode.querySelector('.portfolio-item'); // Get the item div


                    // Set before/after images
                    if (item.beforePhotoUrl) {
                        beforeImg.src = item.beforePhotoUrl;
                        beforeImg.style.display = '';
                    }
                    if (item.afterPhotoUrl) {
                        afterImg.src = item.afterPhotoUrl;
                        afterImg.style.display = '';
                    }
                    // Set hashtags
                    let itemHashtagsText = '';
                    let itemMatchesSelectedHashtag = false;
                    if (item.portfolioItemHashtags && item.portfolioItemHashtags.length) {
                        console.log('Item hashtags:', item.portfolioItemHashtags);
                        itemHashtagsText = item.portfolioItemHashtags.map(h => {
                            const tag = `#${h.hashtag.tag}`;
                            if (selectedHashtag && tag.toLowerCase() === selectedHashtag.toLowerCase()) {
                                itemMatchesSelectedHashtag = true;
                                return `<span class="highlight">${tag}</span>`; // Add highlight class
                            }
                            return tag;
                        }).join(' ');
                        hashtagsSpan.innerHTML = itemHashtagsText; // Use innerHTML to render highlight span
                    } else {
                         hashtagsSpan.textContent = '';
                    }

                     // Set faded background on the item itself
                     if (board.themeColor) {
                         portfolioItemDiv.style.setProperty('--item-bg', fadeColor(board.themeColor, 0.10));
                     } else {
                        portfolioItemDiv.style.setProperty('--item-bg', ''); // Use default if no theme color
                     }

                    // Highlight the entire item if it contains the selected hashtag
                    if (itemMatchesSelectedHashtag) {
                         portfolioItemDiv.classList.add('item-highlight'); // Add class for item highlighting
                    } else {
                         portfolioItemDiv.classList.remove('item-highlight');
                    }


                    // Set price
                    priceSpan.textContent = `$${item.price.toFixed(2)}`;

                    itemsContainer.appendChild(itemNode);
                }
                 // After rendering items, if there are no items in the original data
                 // (this might happen if backend filters items but not boards),
                 // hide the see more button and title image.
                if (!board.items || board.items.length === 0) {
                     seeMoreBtn.style.display = 'none';
                     boardTitleImage.style.display = 'none';
                     itemsContainer.style.display = ''; // Ensure items container is shown even if empty
                }


            }
            // The board is always appended here now because the filtering 
            // happened on the 'boards' list before calling this function.
             boardsContainer.appendChild(boardNode);
        }
    }


    // Hashtag search input event listener
    searchInput.addEventListener('input', async (e) => {
        const query = e.target.value.trim();
        if (query.length >= 2) {
            // Fetch suggestions from the backend
            // Assuming a new endpoint /api/hashtags?q={query} exists
            const res = await fetch(`/api/hashtags?q=${encodeURIComponent(query)}`);
            if (res.ok) {
                const hashtags = await res.json();
                displayHashtagSuggestions(hashtags);
            } else {
                // Clear suggestions if fetch fails
                displayHashtagSuggestions([]);
            }
        } else {
            // Clear suggestions if query is less than 2 characters
            displayHashtagSuggestions([]);
        }
    });

    // Function to display hashtag suggestions
    function displayHashtagSuggestions(hashtags) {
        const query = searchInput.value.trim().toLowerCase().replace(/^#/, ''); // Get current input query, lowercase, remove leading #
        hashtagSuggestionsUl.innerHTML = ''; // Clear previous suggestions

        // Filter hashtags based on the current query
        const matchingHashtags = hashtags.filter(hashtag => 
            hashtag.tag.toLowerCase().startsWith(query)
        );

        if (matchingHashtags.length > 0 && query.length >= 2) { // Only show if there are matches and query is >= 2 chars
            matchingHashtags.forEach(hashtag => {
                const li = document.createElement('li');
                li.classList.add('list-group-item', 'list-group-item-action');
                li.textContent = `#${hashtag.tag}`; // Assuming hashtag object has a 'tag' property
                li.style.cursor = 'pointer';
                li.addEventListener('click', () => {
                    searchInput.value = `#${hashtag.tag}`; // Set input value to selected hashtag
                    hashtagSuggestionsUl.classList.add('d-none'); // Hide suggestions
                    filterBoards(`#${hashtag.tag}`); // Filter boards by selected hashtag
                });
                hashtagSuggestionsUl.appendChild(li);
            });
            hashtagSuggestionsUl.classList.remove('d-none'); // Show suggestions list
        } else {
            hashtagSuggestionsUl.classList.add('d-none'); // Hide suggestions list if empty or query too short
        }
    }

    // Hide suggestions when clicking outside
    document.addEventListener('click', (e) => {
        if (!searchInput.contains(e.target) && !hashtagSuggestionsUl.contains(e.target)) {
            hashtagSuggestionsUl.classList.add('d-none');
        }
    });

    // Initial load of boards
    loadBoards();
});

// Add necessary CSS for highlighting (will add to site.css later)
// .highlight { font-weight: bold; color: var(--highlight-color, blue); } /* Example highlight */
// .item-highlight { border: 2px solid var(--highlight-border-color, blue); } /* Example item highlight */ 