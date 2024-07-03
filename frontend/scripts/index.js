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
let startResetButton = document.getElementById('startResetButton');
let eventCountTxt = document.getElementById('eventCountDisplay');
let SacrificeButton = document.getElementById('Sacrifice');

//Event Listeners\\
document.getElementById('logout-button').addEventListener('click', logout);
startResetButton.addEventListener('click', startOrResetSim);
SacrificeButton.addEventListener('click', retrieveEventData)

//Variables\\
let hasSimStarted = false;
let pollingIntervalId;
let testData = [
  {id: "12344", description: "", type: "sickness"},
  {id: "45666", description: "", type: "death"},
  {id: "34562", description: "", type: "marriage"},
  {id: "13567", description: "", type: "breakage"},
  {id: "56789", description: "", type: "sickness"},
  {id: "09875", description: "", type: "birth"},
  {id: "43567", description: "", type: "marriage"},
  {id: "12678", description: "", type: "birth"}
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
let timeDisplay = document.getElementById('timeDisplay');

// Update the timer every second
setInterval(updateTimer, 5000);

//checkToSeeIfSimulationHasStarted();


//Start and Reset Logic\\
async function checkToSeeIfSimulationHasStarted(){
  let response = await fetchWithAuth('/', {
    method: 'GET',
    headers: {'Content-Type': 'application/json'}
  });

  let tasks = await response.json();

  if(tasks.start){
    hasSimStarted = true;
    startResetButton.value = false;
    startResetButton.textContent = "Reset Simulation";
    if(startResetButton.classList.contains('button-green')){
      startResetButton.classList.remove('button-green');
      startResetButton.classList.add('button-red');
    }
    startPolling();
  }else{
    hasSimStarted = false;
    startResetButton.value = true;
    startResetButton.textContent = "Start Simulation";
    if(startResetButton.classList.contains('button-red')){
      startResetButton.classList.remove('button-red');
      startResetButton.classList.add('button-green');
    }
    stopPolling();
  }
}

async function startOrResetSim(){
  startResetButton.disabled = true;
  let data = {action: startResetButton.value};

  try{
    const response = await fetchWithAuth('/reset', {
      method: 'POST',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      throw new Error("API error: " + response.text());
    }
  }catch (err){
    //something went wrong pop-up
  }

  switch(data.action){
    case "true":
      startResetButton.value = "false";
      startResetButton.textContent = "Reset Simulation";
      if(startResetButton.classList.contains('button-green')){
        startResetButton.classList.remove('button-green');
        startResetButton.classList.add('button-red');
      }
      break;
    case "false":
      startResetButton.value = "true";
      startResetButton.textContent = "Start Simulation";
      if(startResetButton.classList.contains('button-red')){
        startResetButton.classList.remove('button-red');
        startResetButton.classList.add('button-green');
      }
      break;
    }

    startResetButton.disabled = false;
  
}

//Retrieving event data\\
async function retrieveEventData(){
  console.log("retrieving data!");
  try {
    const response = await fetchWithAuth('/get-events');
    console.log(response);
    const responseBody = await response.json(); // Parse the response body as JSON
    console.log("responseBody: " + responseBody);

    if (!response.ok) {
      console.error("Error response:", responseBody);
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    console.log("responseBody: " + responseBody.items);
    const newData = JSON.parse(responseBody.items); // Parse the body JSON string

    console.log("Data received:", newData);

    // Process newData
    if (newData.items.length >= 1) {
      newData.items.forEach(event => {
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
  return testData.some(existingEvent => existingEvent.id === event.id);
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
  idLink.textContent = eventData.id;
  newEvent.appendChild(idLink);

  // Create and append the second <a> element
  const descriptionLink = document.createElement('a');
  descriptionLink.textContent = addRandomDescriptionToEvent(eventData.description);
  newEvent.appendChild(descriptionLink);

  // Create and append the third <a> element with the "pill" class
  const pillLink = document.createElement('a');
  pillLink.textContent = eventData.type;
  pillLink.classList.add('pill');
  newEvent.appendChild(pillLink);
  switch( eventData.type){
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

function addRandomDescriptionToEvent(eventType){
  let description;
  switch(eventType){
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

function randomPicker(list){
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

async function sacrificePersona(){
  const response = await fetchWithAuth('/');
    if (!response.ok) {
      throw new Error(`HTTP error! Status: ${response.status}`);
    }
    const data = await response.json();

    if(data){

    }else{
      
    }
}

