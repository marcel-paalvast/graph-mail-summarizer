import { useCallback, useMemo, useState } from 'react';
import './App.css';
import ApiSettings from './api/ApiSettings';
import Api from './api/Api';
import { AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from '@azure/msal-react';
import { loginRequest } from './auth/loginRequest';
import { BrowserAuthError, InteractionRequiredAuthError } from '@azure/msal-browser';
import { Box, Button, CircularProgress } from '@mui/material';
import { DateCalendar, LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import dayjs, { Dayjs } from 'dayjs';
import Typography from '@mui/material/Typography';
import { GenerateMailOptions } from './models/GenerateMailOptions';

const apiConfig: ApiSettings = {
  baseUri: process.env.REACT_APP_BASE_URI,
}

function App() {
  const { instance, accounts } = useMsal();

  const [today] = useState(dayjs().startOf('day'));
  const [minDate, setMinDate] = useState<Dayjs | null>(today.add(-1, 'week'));
  const [maxDate, setMaxDate] = useState<Dayjs | null>(today);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<string>();

  const requestAccessToken = useCallback(async (): Promise<string> => {
    const request = {
      ...loginRequest,
      account: accounts[0]
    };

    try {
      const response = await instance.acquireTokenSilent(request);
      return response.accessToken;
    } catch (e) {
      if (e instanceof InteractionRequiredAuthError || e instanceof BrowserAuthError) {
        await instance.acquireTokenRedirect(request);
      }
      throw (e);
    }
  }, [accounts, instance]);

  const Login = useCallback(async () => {
    await requestAccessToken();
  }, [requestAccessToken]);

  const Logout = useCallback(async () => {
    await instance.logoutRedirect();
  }, [instance]);

  const api = useMemo(() => new Api(apiConfig, requestAccessToken), [
    requestAccessToken,
  ]);

  const Summarize = useCallback(async () => {
    setLoading(true);

    const options: GenerateMailOptions = {
      from: minDate ?? undefined,
      to: maxDate?.add(1, 'day') ?? undefined,
    }

    try {
      await api.GenerateSummary(options);
      setResult("You'll receive the summary shortly through mail!");
    } catch (e) {
      if (typeof e === 'string') {
        setResult(e);
      }
      setLoading(false);
    }
  }, [api, maxDate, minDate]);

  return (
    <>
      <Box className='bar' width={1} height='64px' display='flex' flexDirection='row-reverse' alignItems='center' padding='0 20px'>
        <AuthenticatedTemplate>
          <Button size='large' onClick={() => Logout()}>Logout</Button>
        </AuthenticatedTemplate>
      </Box>
      <Box className='presentation' display='flex' justifyContent='center'>
        <Box className='main' display='flex' justifyContent='center' flexWrap={'wrap'} maxWidth={'800px'} pb={'80px'}>
          <Typography className='title' color='primary' variant='h3' p={'0 0 80px 0'} width={1} textAlign='center'>
            Mail Summarizer
          </Typography>
          <Typography className='description' width={1} pb={'80px'}>
            Mail Summarizer is a web application that helps you manage your emails more efficiently by generating brief summaries of their contents.
            Save yourself time by getting a consise and organized overview of all the mails you might have missed while you were away!
            <br /><br />
            Using the application is easy. Simple connect to your email account you want summarized.
            Select the start and end date to filter a date range and let the app analyze your mailbox.
            Within a moment you'll receive a summary that captures the key information and main ideas in your inbox.
          </Typography>
          <AuthenticatedTemplate>
            <LocalizationProvider dateAdapter={AdapterDayjs}>
              <Box className='startDate'>
                <Typography variant='subtitle2' color='primary' pl={3}>From</Typography>
                <DateCalendar
                  value={minDate}
                  onChange={(newValue) => setMinDate(newValue)}
                  maxDate={maxDate}
                  views={['day']}
                  sx={{
                    padding: '0 18px',
                  }} />
              </Box>
              <Box className='endDate'>
                <Typography variant='subtitle2' color='primary' pl={3}>Till</Typography>
                <DateCalendar
                  value={maxDate}
                  onChange={(newValue) => setMaxDate(newValue)}
                  maxDate={today}
                  minDate={minDate}
                  views={['day']}
                  sx={{
                    padding: '0 18px',
                  }} />
              </Box>
            </LocalizationProvider>
            <Box sx={{
              display: 'flex',
              justifyContent: 'center',
              width: '100%'
            }}>
              {
                !result && (loading ?
                  <CircularProgress size={32} /> :
                  <Button size='large' onClick={() => Summarize()}>Summarize</Button>)
              }
              {
                result &&
                <Typography color="primary">{result}</Typography>
              }
            </Box>
          </AuthenticatedTemplate>

          <UnauthenticatedTemplate>
            <Box sx={{
              display: 'flex',
              justifyContent: 'center',
              width: '100%'
            }}>
              <Button size='large' onClick={() => Login()}>Login</Button>
            </Box>
          </UnauthenticatedTemplate>
        </Box>
      </Box>
    </>
  );
}

export default App;
