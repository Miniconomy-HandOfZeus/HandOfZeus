import { getEmail, logout } from "./authManager.js";
import { fetchWithAuth } from "./apiHandler.js";

//document.addEventListener('DOMContentLoaded', async () => {console.log(`Email: ${await getEmail()}`);});

document.getElementById('logout-button').addEventListener('click', logout);

// console.log(await fetchWithAuth('/helloworld', { 
//   method: 'GET',
//   headers: {'Content-Type': 'application/json'}
// }));

const testData = [
  {id: "12344", description: "", type: "sickness"},
  {id: "45666", description: "", type: "death"},
  {id: "34562", description: "", type: "marriage"},
  {id: "13567", description: "", type: "breakage"},
  {id: "56789", description: "", type: "sickness"},
  {id: "09875", description: "", type: "birth"},
  {id: "43567", description: "", type: "marriage"},
  {id: "12678", description: "", type: "birth"}
]

let timeDisplay = document.getElementById('timeDisplay');
// Constants for time scaling
const REAL_TO_SIMULATION_RATIO = 24 * 60 * 60 * 1000 / (2 * 365 * 24 * 60 * 60 * 1000); // 24 hours in real-time to 2 years in simulation time

// Assuming you have received the start time from your API
const startTimeFromApi = new Date('2024-07-01T12:00:00Z'); // Replace with the actual start time from your API

function updateTimer() {
  const currentTime = new Date();
  const timeDifference = currentTime - startTimeFromApi;

  // Convert real-time to simulation time
  const simulationTimeElapsed = timeDifference * REAL_TO_SIMULATION_RATIO;

  // Calculate years, months, and days in simulation time
  const simulationYears = Math.floor(simulationTimeElapsed / (365 * 24 * 60 * 60 * 1000));
  const remainingDays = simulationTimeElapsed % (365 * 24 * 60 * 60 * 1000);
  const simulationMonths = Math.floor(remainingDays / (30 * 24 * 60 * 60 * 1000));
  const simulationDays = Math.floor((remainingDays % (30 * 24 * 60 * 60 * 1000)) / (24 * 60 * 60 * 1000));

  // Format simulation time with leading zeros
  const formattedYears = simulationYears.toString().padStart(2, '0');
  const formattedMonths = simulationMonths.toString().padStart(2, '0');
  const formattedDays = simulationDays.toString().padStart(2, '0');

  // Display the formatted simulation time
  const formattedTime = `${formattedYears}/${formattedMonths}/${formattedDays}`;
  console.log(formattedTime); // Output to console for verification
  timeDisplay.textContent = formattedTime;
}

// Update the timer every second
//setInterval(updateTimer, 5000);

// Function to add a new event element
function addEventElement(eventData) {
  // Create a new section element
  const newEvent = document.createElement('section');
  newEvent.classList.add('eventObject');

  // Create and append the first <a> element
  const idLink = document.createElement('a');
  idLink.textContent = eventData.id;
  newEvent.appendChild(idLink);

  // Create and append the second <a> element
  const descriptionLink = document.createElement('a');
  descriptionLink.textContent = eventData.description;
  newEvent.appendChild(descriptionLink);

  // Create and append the third <a> element with the "pill" class
  const pillLink = document.createElement('a');
  pillLink.textContent = eventData.type;
  pillLink.classList.add('pill');
  newEvent.appendChild(pillLink);
  switch(type){
    case 'sickness':
      pillLink.classList.add('pill-blue');
    case 'death':
      pillLink.classList.add('pill-red');
      
    case 'birth':
      pillLink.classList.add('pill-green');
      
    case 'marriage':
      pillLink.classList.add('pill-yellow');
      
    case 'hunger':
      pillLink.classList.add('pill-lightBlue');
      
    case 'breakage':
      pillLink.classList.add('pill-orange');
      
    case 'fired':
      pillLink.classList.add('pill-red');
      
    case 'Famine':
      pillLink.classList.add('pill-purple');
      
    case 'plague':
      pillLink.classList.add('pill-purple');
      
    case 'apocalypse':
      pillLink.classList.add('pill-purple');
      
    case 'war':
      pillLink.classList.add('pill-purple');
      
    default:
      pillLink.classList.add('pill-blue');
  }
  
  // Append the new section to the existing eventHolder section
  document.getElementById('eventHolder').appendChild(newEvent);
}
