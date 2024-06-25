import { getEmail, logout } from "./authManager.js";

document.addEventListener('DOMContentLoaded', async () => {console.log(`Email: ${await getEmail()}`);});

document.getElementById('logout-button').addEventListener('click', logout);