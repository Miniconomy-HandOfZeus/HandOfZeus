const REAL_TO_SIMULATION_RATIO = 24 * 60 * 60 * 1000 / (2 * 365 * 24 * 60 * 60 * 1000); // 24 hours in real-time to 2 years in simulation time
let startTimeFromApi = new Date('2024-07-01T12:00:00Z');

function updateTimer() {
    const currentTime = new Date();
    const timeDifference = currentTime - startTimeFromApi;

    const simulationTimeElapsed = timeDifference * REAL_TO_SIMULATION_RATIO;

    // Calculate years, months, and days in simulation time
    const simulationYears = Math.floor(simulationTimeElapsed / (365 * 24 * 60 * 60 * 1000));
    const remainingDays = simulationTimeElapsed % (365 * 24 * 60 * 60 * 1000);
    const simulationMonths = Math.floor(remainingDays / (30 * 24 * 60 * 60 * 1000));
    const simulationDays = Math.floor((remainingDays % (30 * 24 * 60 * 60 * 1000)) / (24 * 60 * 60 * 1000));

    const formattedYears = simulationYears.toString().padStart(2, '0');
    const formattedMonths = simulationMonths.toString().padStart(2, '0');
    const formattedDays = simulationDays.toString().padStart(2, '0');

    const formattedTime = `${formattedYears}/${formattedMonths}/${formattedDays}`;
    console.log(formattedTime); 
    timeDisplay.textContent = formattedTime;
}

export { updateTimer, startTimeFromApi };