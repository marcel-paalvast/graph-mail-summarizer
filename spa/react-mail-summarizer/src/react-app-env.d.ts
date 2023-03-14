/// <reference types="react-scripts" />

declare namespace NodeJS {
    interface ProcessEnv {
        REACT_APP_BASE_URI: string,

        REACT_APP_GRAPH_TENANT_ID: string,
        REACT_APP_GRAPH_CLIENT_ID: string,
    }
}
