import { clientId, cognitoDomain, redirectUri } from './apiConfig.js';

const queryString = window.location.search;
const urlParams = new URLSearchParams(queryString);
const code = urlParams.get('code');
const state = urlParams.get('state');

if (code && state) {
  // Retrieve code state and verifier
  const cookies = document.cookie.split(';');
  let savedState = '';
  let savedCodeVerifier = '';
  cookies.forEach(cookie => {
    const [key, value] = cookie.split('=');
    if (key.trim() === 'state') {
      savedState = value;
    } else if (key.trim() === 'codeVerifier') {
      savedCodeVerifier = value;
    }
  });
  
  if (savedState === state){
    await exchangeAuthorizationCodeForTokens(code, savedCodeVerifier);
    window.location.href = "/views/index.html";
  } else {
    console.error('State mismatch');
    window.location.href = "/views/login.html";
  }
}

 // Function to exchange authorization code for tokens
 async function exchangeAuthorizationCodeForTokens(code, codeVerifier) {
  const tokenEndpoint = `https://${cognitoDomain}/oauth2/token`;
  const body = new URLSearchParams({
    grant_type: 'authorization_code',
    client_id: clientId,
    code_verifier: codeVerifier,
    code: code,
    redirect_uri: redirectUri
  });

  try {
    const response = await fetch(tokenEndpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: body
    });

    const tokens = await response.json();
    sessionStorage.setItem('accessToken', tokens.access_token);
    sessionStorage.setItem('idToken', tokens.id_token);
    sessionStorage.setItem('refreshToken', tokens.refresh_token);
  } catch (error) {
    console.error('Token exchange failed:', error);
  }
}