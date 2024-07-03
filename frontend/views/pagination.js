// Sample data array (simulating data fetched from a database)
let data = [
    { id: 1, content: 'Item 1' },
    { id: 2, content: 'Item 2' },
    { id: 3, content: 'Item 3' },
    { id: 4, content: 'Item 4' },
    { id: 5, content: 'Item 5' },
    { id: 6, content: 'Item 6' },
    { id: 7, content: 'Item 7' },
    { id: 8, content: 'Item 8' },
    { id: 9, content: 'Item 9' },
    { id: 10, content: 'Item 10' },
    { id: 11, content: 'Item 11' },
    { id: 12, content: 'Item 12' },
    // Add more data as needed
  ];
  
  const itemsPerPage = 5; // Number of items per page
  let currentPage = 1; // Current page, starting with the first page
  
  const dataList = document.getElementById('data-list');
  const paginationButtons = document.getElementById('pagination-buttons');
  
  // Function to display items for the current page
  function displayItems(page) {
    // Clear previous items
    dataList.innerHTML = '';
  
    // Calculate start and end indices for the current page
    const startIndex = (page - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
  
    // Get items for the current page
    const pageItems = data.slice(startIndex, endIndex);
  
    // Display items
    pageItems.forEach(item => {
      const itemElement = document.createElement('div');
      itemElement.classList.add('item');
      itemElement.textContent = item.content;
      dataList.appendChild(itemElement);
    });
  }
  
  // Function to create pagination buttons
  function createPaginationButtons() {
    paginationButtons.innerHTML = '';
  
    const totalPages = Math.ceil(data.length / itemsPerPage);
  
    for (let i = 1; i <= totalPages; i++) {
      const button = document.createElement('button');
      button.textContent = i;
      button.addEventListener('click', () => {
        currentPage = i;
        displayItems(currentPage);
      });
      paginationButtons.appendChild(button);
    }
  }
  
  // Function to add a new item to the beginning of the data array
  function addNewItem(newItem) {
    data.unshift(newItem);
  
    // Adjust current page if necessary
    if (currentPage === 1) {
      displayItems(currentPage); // Re-display current page to show new item
    } else {
      currentPage = 1; // Reset to the first page to show the newest item
      displayItems(currentPage);
      createPaginationButtons(); // Recreate pagination buttons since data structure changed
    }
  }
  
  // Simulating addition of new items (replace with actual data fetching or user input)
  setTimeout(() => {
    addNewItem({ id: 13, content: 'New Item 13' }); // Example of adding a new item
  }, 3000); // Simulating after 3 seconds
  
  // Initial display
  displayItems(currentPage);
  createPaginationButtons();
  