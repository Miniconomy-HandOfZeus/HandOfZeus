const hostname = window.location.hostname;
const clientId = 'juutjq7cpv2nrgon8re8fr0td';
const cognitoDomain = 'zeus.auth.eu-west-1.amazoncognito.com';
let backendUrl = 'https://api.zeus.projects.bbdgrad.com';
let redirectUri = 'https://zeus.projects.bbdgrad.com/views/callback.html';
let logoutRedirectUri = 'https://zeus.projects.bbdgrad.com/views/logout.html';
if (hostname.includes("localhost") || hostname.includes("127.0.0.1")) {
  backendUrl = 'https://localhost:8080';
  redirectUri = 'http://localhost:5500/views/callback.html';
  logoutRedirectUri = 'http://localhost:5500/views/logout.html';
}

export { backendUrl, redirectUri, logoutRedirectUri, clientId, cognitoDomain };