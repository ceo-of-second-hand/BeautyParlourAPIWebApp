// add.js

document.addEventListener('DOMContentLoaded', () => {
    const boardForm = document.getElementById('add-board-form');
    const itemForm = document.getElementById('add-item-form');
    const boardNameInput = document.getElementById('board-name');
    const itemBoardSelect = document.getElementById('item-board');
    const itemHashtagsInput = document.getElementById('item-hashtags');
    const itemPriceInput = document.getElementById('item-price');
    const boardThemeColorInput = document.getElementById('board-theme-color');
    const itemTitleImageToggle = document.getElementById('item-title-image');
    const itemBeforePhotoInput = document.getElementById('item-before-photo');
    const itemAfterPhotoInput = document.getElementById('item-after-photo');

    if (!itemBoardSelect || !itemForm || !boardForm) {
        console.error('Error: Could not find required elements');
        return;
    }

    // Load boards for dropdown
    async function loadBoards() {
        const res = await fetch('/api/boards');
        if (!res.ok) return;
        const boards = await res.json();
        itemBoardSelect.innerHTML = boards.map(b => `<option value="${b.id}">${b.name}</option>`).join('');
    }

    loadBoards();

    // Add board
    boardForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const name = boardNameInput.value.trim();
        const themeColor = boardThemeColorInput.value;
        if (!name) return;
        const res = await fetch('/api/boards', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name, themeColor })
        });
        if (res.ok) {
            boardNameInput.value = '';
            boardThemeColorInput.value = '#ff69b4';
            await loadBoards();
            alert('Board added!');
        } else {
            alert('Failed to add board.');
        }
    });

    // Only handle before/after photo and title image checkbox for portfolio item
    itemForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        console.log('Attempting to add portfolio item...');

        /*
        // Get the board select element directly within the submit handler
        const itemBoardSelect = document.getElementById('item-board');
        console.log('Item board select element:', itemBoardSelect);
        */

        if (!itemBoardSelect) {
            console.error('Error: Board select element not found.');
            alert('Error: Board selection is unavailable.');
            return;
        }

        const boardId = parseInt(itemBoardSelect.value);
        const beforePhotoFile = itemBeforePhotoInput.files[0];
        const afterPhotoFile = itemAfterPhotoInput.files[0];
        const hashtags = itemHashtagsInput.value.split(',').map(h => h.trim().replace(/^#/, '')).filter(Boolean);
        const price = parseFloat(itemPriceInput.value);
        const useAsTitleImage = itemTitleImageToggle.checked;

        console.log(`boardId: ${boardId}`);
        console.log(`beforePhotoFile: ${beforePhotoFile ? beforePhotoFile.name : 'none'}`);
        console.log(`afterPhotoFile: ${afterPhotoFile ? afterPhotoFile.name : 'none'}`);
        console.log(`hashtags: ${hashtags.join(', ')}`);
        console.log(`price: ${price}`);
        console.log(`useAsTitleImage: ${useAsTitleImage}`);

        if (!boardId || !afterPhotoFile || !hashtags.length || isNaN(price)) {
            console.error('Validation failed: Missing required fields or invalid price.');
            // Add a user-friendly alert or message here if needed
            alert('Please fill in all required fields (After Photo, Hashtags, Price) and select a board.');
            return;
        }

        console.log('Validation successful. Proceeding with upload...');

        // 1. Upload before photo if provided
        let beforePhotoUrl = null;
        if (beforePhotoFile) {
            console.log('Uploading before photo...');
            const beforeFormData = new FormData();
            beforeFormData.append('image', beforePhotoFile);
            try {
                const beforeUploadRes = await fetch('/api/portfolioitems/upload-image', {
                    method: 'POST',
                    body: beforeFormData
                });
                if (!beforeUploadRes.ok) {
                    const errorText = await beforeUploadRes.text();
                    console.error(`Before photo upload failed: ${beforeUploadRes.status} ${beforeUploadRes.statusText} - ${errorText}`);
                    alert('Before photo upload failed.');
                    return;
                }
                beforePhotoUrl = (await beforeUploadRes.json()).imageUrl;
                console.log(`Before photo uploaded: ${beforePhotoUrl}`);
            } catch (error) {
                console.error('Error during before photo upload:', error);
                alert('Error during before photo upload.');
                return;
            }
        }

        // 2. Upload after photo (required)
        console.log('Uploading after photo...');
        const afterFormData = new FormData();
        afterFormData.append('image', afterPhotoFile);
        try {
            const afterUploadRes = await fetch('/api/portfolioitems/upload-image', {
                method: 'POST',
                body: afterFormData
            });
            if (!afterUploadRes.ok) {
                const errorText = await afterUploadRes.text();
                console.error(`After photo upload failed: ${afterUploadRes.status} ${afterUploadRes.statusText} - ${errorText}`);
                alert('After photo upload failed.');
                return;
            }
            const { imageUrl: afterPhotoUrl } = await afterUploadRes.json(); // Destructure and rename
            console.log(`After photo uploaded: ${afterPhotoUrl}`);

            // 3. Create item
            console.log('Creating portfolio item...');
            const item = {
                boardId,
                beforePhotoUrl,
                afterPhotoUrl,
                price,
                hashtags: hashtags.map(tag => ({ tag }))
            };

            const res = await fetch('/api/portfolioitems', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(item)
            });

            if (res.ok) {
                console.log('Portfolio item created successfully.');
                // 4. If use as title image, update board with after photo
                if (useAsTitleImage) {
                    console.log('Setting after photo as board title image...');
                    await fetch(`/api/boards/${boardId}`, {
                        method: 'PUT',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ titleImageUrl: afterPhotoUrl })
                    });
                    // Check if board update was successful (optional but good practice)
                    // if (!boardUpdateRes.ok) { console.error('Failed to update board title image'); }
                    console.log('Board title image updated.');
                }

                itemBeforePhotoInput.value = '';
                itemAfterPhotoInput.value = '';
                itemHashtagsInput.value = '';
                itemPriceInput.value = '';
                itemTitleImageToggle.checked = false;
                alert('Portfolio item added!');

            } else {
                const errorText = await res.text();
                console.error(`Failed to add item: ${res.status} ${res.statusText} - ${errorText}`);
                alert('Failed to add item.');
            }

        } catch (error) {
            console.error('Error during after photo upload or item creation:', error);
            alert('Error during photo upload or item creation.');
            return; // Stop processing if after photo fails
        }
    });
}); 