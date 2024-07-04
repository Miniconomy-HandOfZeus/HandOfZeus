import { getEmail, logout } from "./authManager.js";
import { fetchWithAuth } from "./apiHandler.js";
import { updateTimer, startTimeFromApi } from "./timeHandler.js";
import {
  deathDescriptions,
  marriedDescriptions,
  birthDescriptions,
  warDescriptions,
  plagueDescriptions,
  famineDescription,
  sicknessDescription,
  apocalypseDescription,
  breakageDescription
} from "./descriptions.js";

//UI elements\\
let startResetButton = document.getElementById('startButton');
let resetButton = document.getElementById('resetButton');
let eventCountTxt = document.getElementById('eventCountDisplay');
let SacrificeButton = document.getElementById('Sacrifice');
let timerTxt = document.getElementById('timeDisplay');

//Event Listeners\\
document.getElementById('logout-button').addEventListener('click', logout);
startResetButton.addEventListener('click', startOrResetSim);
resetButton.addEventListener('click', startOrResetSim);
SacrificeButton.addEventListener('click', sacrificeSomeone); 

//Variables\\
let hasSimStarted = false;
let pollingIntervalId;
let testData = [
  { id: "12344", description: "", type: "sickness" },
  { id: "45666", description: "", type: "death" },
  { id: "34562", description: "", type: "marriage" },
  { id: "13567", description: "", type: "breakage" },
  { id: "56789", description: "", type: "sickness" },
  { id: "09875", description: "", type: "birth" },
  { id: "43567", description: "", type: "marriage" },
  { id: "12678", description: "", type: "birth" }
]

const eventTypes = {
  Sickness: "sickness",
  Death: "death",
  Marriage: "marriage",
  Birth: "birth",
  Breakage: "breakage",
  Plague: "plague",
  Famine: "famine",
  War: "war",
  Apocalypse: "apocalypse"

}

eventCountTxt.innerText = testData.length;

//Time stuff\\
let simulationStartDate;

function calculateDate() {
  const currentDate = new Date();
  // Calculate the difference in seconds
  const secondsDifference = (currentDate - simulationStartDate) / 1000;

  // Get the current day of the simulation (e.g., day 1302)
  const simulationDayNumber = Math.floor((secondsDifference / 120) + 1);

  // Calculate current year
  const year = Math.floor(simulationDayNumber / 360) + 1;
  const daysIntoYear = simulationDayNumber % 360;

  // Calculate current month and day
  const month = Math.floor(daysIntoYear / 30) + 1;
  const day = daysIntoYear % 30;

  // Format the year, month, and day with leading zeros
  const formattedDate = `${String(year).padStart(2, '0')}|${String(month).padStart(2, '0')}|${String(day).padStart(2, '0')}`;
  console.log(formattedDate);
  timerTxt.innerText = formattedDate;
}

// Update the timer every second
if(hasSimStarted){
  setInterval(calculateDate, 5000);
}

checkToSeeIfSimulationHasStarted();

//Start and Reset Logic\\
async function checkToSeeIfSimulationHasStarted(){
  let response = await fetchWithAuth('/sim-started', {
    method: 'GET',
    headers: { 'Content-Type': 'application/json' }
  });

  let tasks = await response.json();
  if(tasks.json.hasSatrted){
    console.log(tasks.json.startTime);
    simulationStartDate = tasks.json.startTime;
    hasSimStarted = true;
    
    startPolling();
  } else {
    hasSimStarted = false;
    
    stopPolling();
  }
}

async function sacrificeSomeone() {
  let randomNum = generateRandomNumber(5);
    try {
      let data = {"number":randomNum};
      const response = await fetchWithAuth(`/sacrifice`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      });
      if (!response.ok) {
        throw new Error("API error: " + response.text());
      }
    } catch (err) {
      alert("Unavailable for sacrifice at this time" + err);
    }
}

function generateRandomNumber(max){
  let random = Math.floor(Math.random() * max) + 1;
  return random
}

async function startOrResetSim() {
  startResetButton.disabled = true;
  let data = { action: startResetButton.value };

  try {
    const response = await fetchWithAuth('/reset', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      throw new Error("API error: " + response.text());
    }

    if(startResetButton.value){
      const responseBody = await response.json();
      console.log(responseBody);
      const newData = responseBody.json;
      console.log(newData);
      console.log(newData.startTime);
      simulationStartDate = newData.startTime;
    }
    
  }catch (err){
    //something went wrong pop-up
  }

  // switch (data.action) {
  //   case "true":
  //     startResetButton.value = "false";
  //     startResetButton.textContent = "Reset Simulation";
  //     if (startResetButton.classList.contains('button-green')) {
  //       startResetButton.classList.remove('button-green');
  //       startResetButton.classList.add('button-red');
  //     }
  //     break;
  //   case "false":
  //     startResetButton.value = "true";
  //     startResetButton.textContent = "Start Simulation";
  //     if (startResetButton.classList.contains('button-red')) {
  //       startResetButton.classList.remove('button-red');
  //       startResetButton.classList.add('button-green');
  //     }
  //     break;
  // }

  startResetButton.disabled = false;

}

//Retrieving event data\\
async function retrieveEventData() {
  console.log("retrieving data!");
  try {
    const response = await fetchWithAuth('/get-events');
    console.log("Response received:", response);

    const responseBody = await response.json(); // Parse the response body as JSON

    if (!response.ok) {
      console.error("Error response:", responseBody);
      throw new Error(`HTTP error! Status: ${response.status}`);
    }

    console.log("Response body:", responseBody); // Log the entire response body object

    const newData = responseBody.items; // Access the 'items' array directly

    console.log("Data received:", newData);

    // Process newData
    if (newData.length >= 1) {
      newData.forEach(event => {
        if (!isDuplicate(testData, event)) {
          testData.unshift(event);
          addEventElement(event);
        }
      });

      eventCountTxt.innerText = testData.length;
    }
  } catch (error) {
    console.error('Error fetching data:', error);
  }
}

function isDuplicate(testData, event) {
  return testData.some(existingEvent => existingEvent.Key === event.Key);
}

function startPolling(interval = 5000) {
  retrieveEventData(); // Initial fetch
  pollingIntervalId = setInterval(retrieveEventData, interval); // Polling interval
}

function stopPolling() {
  clearInterval(pollingIntervalId);
}

//creating event element\\
function addEventElement(eventData) {
  // Create a new section element
  const newEvent = document.createElement('section');
  newEvent.classList.add('eventObject');

  // Create and append the first <a> element
  const idLink = document.createElement('a');
  idLink.textContent = calculateDateEvent(simulationStartDate, eventData.date);
  newEvent.appendChild(idLink);

  // Create and append the second <a> element
  const descriptionLink = document.createElement('a');
  descriptionLink.textContent = addRandomDescriptionToEvent(eventData.event_name);
  newEvent.appendChild(descriptionLink);

  // Create and append the third <a> element with the "pill" class
  const pillLink = document.createElement('a');
  pillLink.textContent = eventData.type;
  pillLink.classList.add('pill');
  newEvent.appendChild(pillLink);
  switch(eventData.event_name){
    case 'Sickness':
      pillLink.classList.add('pill-blue');

    case 'Death':
      pillLink.classList.add('pill-red');

    case 'Birth':
      pillLink.classList.add('pill-green');
      
    case 'Marriage':
      pillLink.classList.add('pill-yellow');
      
    case 'Hunger':
      pillLink.classList.add('pill-lightBlue');
      
    case 'Breakage':
      pillLink.classList.add('pill-orange');
      
    case 'Fired':
      pillLink.classList.add('pill-red');

    case 'Famine':
      pillLink.classList.add('pill-purple');
      
    case 'Plague':
      pillLink.classList.add('pill-purple');
      
    case 'Apocalypse':
      pillLink.classList.add('pill-purple');
      
    case 'War':
      pillLink.classList.add('pill-purple');

    default:
      pillLink.classList.add('pill-blue');
  }

  // Append the new section to the existing eventHolder section
  document.getElementById('eventHolder').appendChild(newEvent);
}

function addRandomDescriptionToEvent(eventType) {
  let description;
  switch (eventType) {
    case eventTypes.Sickness:
      description = randomPicker(sicknessDescription);
      return description;
    case eventTypes.Birth:
      description = randomPicker(birthDescriptions);
      return description;
    case eventTypes.Death:
      description = randomPicker(deathDescriptions);
      return description;
    case eventTypes.Marriage:
      description = randomPicker(marriedDescriptions);
      return description;
    case eventTypes.Breakage:
      description = randomPicker(breakageDescription);
      return description;
    case eventTypes.Famine:
      description = randomPicker(famineDescription);
      return description;
    case eventTypes.Plague:
      description = randomPicker(plagueDescriptions);
      return description;
    case eventTypes.War:
      description = randomPicker(warDescriptions);
      return description;
    case eventType.Apocalypse:
      description = randomPicker(apocalypseDescription);
      return description;
    default:
      description = "Something happened but hand of Zeus has no idea what"
      return description;
  }
}

function randomPicker(list) {
  if (list.length === 0) {
    return null;
  }
  const randomIndex = Math.floor(Math.random() * list.length);
  return list[randomIndex];
}

//Event data filter stuff\\
const filterType = document.getElementById('filterType');
const eventList = document.getElementById('eventList');
const allEvents = Array.from(eventList.getElementsByClassName('eventObject'));

filterType.addEventListener('change', () => {
  const selectedType = filterType.value;
  filterEvents(selectedType);
});

function filterEvents(type) {
  eventList.innerHTML = ''; // Clear current list
  allEvents.forEach(event => {
    const eventType = event.querySelector('.pill').textContent;
    if (type === 'all' || eventType === type) {
      eventList.appendChild(event);
    }
  });
}

filterEvents('all');

//human sacrifice\\

async function sacrificePersona() {
  const response = await fetchWithAuth('/');
  if (!response.ok) {
    throw new Error(`HTTP error! Status: ${response.status}`);
  }
  const data = await response.json();

  if (data) {

  } else {

  }
}

function calculateDateEvent(simulationStartDate, currentDate) {
  // Convert the dates to JavaScript Date objects if they are not already
  simulationStartDate = new Date(simulationStartDate);
  currentDate = new Date(currentDate);

  // Get the total seconds difference between the dates
  const totalSeconds = (currentDate - simulationStartDate) / 1000;

  // Get the current day of the simulation (e.g., day 1302)
  const simulationDayNumber = Math.floor(totalSeconds / 120) + 1;

  // Calculate current year
  const year = Math.floor(simulationDayNumber / 360) + 1;
  const daysIntoYear = Math.floor(simulationDayNumber % 360);

  // Calculate current month and day
  const month = Math.floor(daysIntoYear / 30) + 1;
  const day = Math.floor(daysIntoYear % 30);

  // Format the year, month, and day with leading zeros
  const formattedDate = `${year.toString().padStart(2, '0')}|${month.toString().padStart(2, '0')}|${day.toString().padStart(2, '0')}`;

  return formattedDate;
}

