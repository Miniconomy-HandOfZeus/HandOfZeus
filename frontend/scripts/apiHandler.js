import { backendUrl } from "./apiConfig";

async function fetchWithAuth(endpoint, options = {}) {
    checkToken();
    const headers = new Headers(options.headers || {});
  
    const token = sessionStorage.getItem('accessToken');

    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }
  
    let url = backendUrl + endpoint;
    let result = await fetch(url, {
      ...options,
      headers,
    });

    return result;
}

async function simpleFetch(endpoint, options = {}) {
    const headers = new Headers(options.headers || {});
  
  
    let url = backendUrl + endpoint;
    let result = await fetch(url, {
      ...options,
      headers,
    });

    return result;
}

module.exports = { fetchWithAuth, simpleFetch };