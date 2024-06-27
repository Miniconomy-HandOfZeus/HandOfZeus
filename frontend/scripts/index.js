import { getEmail, logout } from "./authManager.js";
import { backendUrl } from "./apiConfig.js";
import { fetchWithAuth } from "./apiHandler.js";

document.addEventListener('DOMContentLoaded', async () => {console.log(`Email: ${await getEmail()}`);});

document.getElementById('logout-button').addEventListener('click', logout);

console.log(await fetchWithAuth(backendUrl + '/helloworld', { 
  method: 'GET',
  headers: {'Content-Type': 'application/json'},
  body: 'Hello world!'
}));