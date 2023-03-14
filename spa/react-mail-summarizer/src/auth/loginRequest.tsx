
// Add scopes here for ID token to be used at Microsoft identity platform endpoints.
export const loginRequest = {
  scopes: [
    `api://${process.env.REACT_APP_GRAPH_CLIENT_ID}/access_as_user`,
    "Mail.Read",
    "Mail.Send",
    "openid",
    "email"
  ]
};
