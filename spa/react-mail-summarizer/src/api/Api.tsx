import { GenerateMailOptions } from "../models/GenerateMailOptions";
import ApiConfig from "./ApiSettings";

export default class Api {
  private config: ApiConfig;
  private GetToken?: () => Promise<string>;

  constructor(config: ApiConfig, getToken?: () => Promise<string>) {
    this.config = config;
    this.GetToken = getToken;
  }

  GenerateSummary(options: GenerateMailOptions): Promise<void> {
    return new Promise((resolve, reject) => {
      const xhttp = new XMLHttpRequest();

      xhttp.onload = (): void => {
        if (xhttp.status === 202) {
          resolve(JSON.parse(xhttp.response));
        } else {
          reject(this.getErrorDescription(xhttp));
        }
      }

      xhttp.onerror = (): void => {
        reject(this.getErrorDescription(xhttp));
      }

      xhttp.open('POST', `${this.config.baseUri}summarize`, true);
      xhttp.setRequestHeader("Content-type", "application/json");
      this.setAuthorizationToken(xhttp).then(x => xhttp.send(JSON.stringify(options)));
    });
  }

  async setAuthorizationToken(request: XMLHttpRequest) {
    if (this.GetToken) {
      const token = await this.GetToken();
      request.setRequestHeader('Authorization', `Bearer ${token}`);
    }
  }

  getErrorDescription(xhttp: XMLHttpRequest) {
    switch (xhttp.status) {
      case 400:
        return xhttp.response;
      case 401:
        return 'Authentication error';
      case 403:
        return 'Forbidden';
      case 404:
        return 'Not found';
      case 500:
        return 'Server error';
      default:
        return 'Unknown network error';
    }
  }
}