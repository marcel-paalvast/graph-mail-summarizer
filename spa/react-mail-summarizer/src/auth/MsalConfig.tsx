export const msalConfig = {
  auth: {
    clientId: process.env.REACT_APP_GRAPH_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${process.env.REACT_APP_GRAPH_TENANT_ID}`,
    redirectUri: "/",
  },
  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: false,
  }
};
