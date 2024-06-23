
const urlBackend = 'https://localhost:8080'

async function fetchWithAuth(endpoint, options = {}) {
    checkToken();
    const headers = new Headers(options.headers || {});
  
    const token = sessionStorage.getItem('idToken');

    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }
  
    let url = apiEndpoint + endpoint;
    let result = await fetch(url, {
      ...options,
      headers,
    });

    return result;
}

async function simpleFetch(endpoint, options = {}) {
    checkToken();
    const headers = new Headers(options.headers || {});
  
  
    let url = apiEndpoint + endpoint;
    let result = await fetch(url, {
      ...options,
      headers,
    });

    return result;
}



module.exports = { fetchWithAuth, simpleFetch };
  