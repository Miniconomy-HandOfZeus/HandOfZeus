import { getEmail, logout } from "./authManager.js";
import { fetchWithAuth } from "./apiHandler.js";

document.addEventListener('DOMContentLoaded', async () => {console.log(`Email: ${await getEmail()}`);});

document.getElementById('logout-button').addEventListener('click', logout);

console.log(await fetchWithAuth('/helloworld', { 
  method: 'GET',
  headers: {'Content-Type': 'application/json'}
}));