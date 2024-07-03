const REAL_TO_SIMULATION_RATIO = 24 * 60 * 60 * 1000 / (2 * 365 * 24 * 60 * 60 * 1000); // 24 hours in real-time to 2 years in simulation time
let startTimeFromApi = new Date('2024-07-01T12:00:00Z');

function updateTimer() {
    const currentTime = new Date();
    console.log(currentTime);
    const timeDifference = currentTime - startTimeFromApi;
    console.log(timeDifference);

    const simulationTimeElapsed = timeDifference * REAL_TO_SIMULATION_RATIO;

    // Calculate years, months, and days in simulation time
    const simulationYears = (simulationTimeElapsed / (365 * 24 * 60 * 60));
    console.log(simulationYears);
    const remainingDays = simulationTimeElapsed % (365 * 24 * 60 * 60);
    const simulationMonths = (remainingDays / (30 * 24 * 60 * 60 ));
    console.log(simulationMonths);
    const simulationDays = ((remainingDays % (30 * 24 * 60 * 60 * 1000)) / (24 * 60 * 60 * 1000));
    console.log(simulationDays);

    const formattedYears = simulationYears.toString().padStart(2, '0');
    const formattedMonths = simulationMonths.toString().padStart(2, '0');
    const formattedDays = simulationDays.toString().padStart(2, '0');

    const formattedTime = `${formattedYears}/${formattedMonths}/${formattedDays}`;
    console.log(formattedTime); 
    timeDisplay.textContent = formattedTime;
}

function calculateDate(simulationStartDate, currentDate) {
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
  
    return formattedDate;
  }

export { updateTimer, startTimeFromApi, calculateDate };