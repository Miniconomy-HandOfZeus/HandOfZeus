import { clientId, cognitoDomain, redirectUri } from './apiConfig.js';

// Helper function to generate a random string of specified length
function generateRandomString(length) {
  const array = new Uint8Array(length);
  window.crypto.getRandomValues(array);
  return Array.from(array, byte => ('0' + byte.toString(36)).slice(-2)).join('');
}

// Helper function to base64 URL encode an ArrayBuffer
function base64UrlEncode(arrayBuffer) {
  const base64 = btoa(String.fromCharCode.apply(null, new Uint8Array(arrayBuffer)));
  return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

async function generateCodeVerifierAndChallenge() {
  const codeVerifier = generateRandomString(64);

  // Hash the code verifier using SHA-256
  const hashed = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(codeVerifier));

  // Base64-urlencode the hash
  const codeChallenge = base64UrlEncode(hashed);

  return { codeVerifier, codeChallenge };
}

document.getElementById('login-button').addEventListener('click', async () => {
  const { codeVerifier, codeChallenge } = await generateCodeVerifierAndChallenge();
  const state = generateRandomString(16);

  // Store the state and code verifier
  document.cookie = `state=${state}; path=/; Secure`;
  document.cookie = `codeVerifier=${codeVerifier}; path=/; Secure`;

  // Redirect to cognito authentication
  const authUrl = `https://${cognitoDomain}/oauth2/authorize?response_type=code&client_id=${clientId}&redirect_uri=${redirectUri}&scope=openid+email+profile+aws.cognito.signin.user.admin&code_challenge_method=S256&code_challenge=${codeChallenge}&state=${state}`;
  window.location.href = authUrl;
});