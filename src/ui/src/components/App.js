import React, { useState, useEffect } from 'react';
import ReactGA from 'react-ga';
import { withAuthenticator, AmplifySignOut } from '@aws-amplify/ui-react';
import { Auth } from 'aws-amplify';
import { Amplify } from 'aws-amplify';
import { Router, Route, Switch } from 'react-router-dom';
import { createBrowserHistory } from 'history';
import { NavMenu } from './NavMenu';
import './../style/App.css';
import config from './../config';
ReactGA.initialize('UA-175994834-1');
ReactGA.pageview(window.location.pathname);

export const history = createBrowserHistory();

Amplify.configure({
  Auth: {
    region: config.AWS_REGION,
    userPoolId: config.AWS_COGNITO_USER_POOL_ID,
    userPoolWebClientId: config.AWS_COGNITO_WEB_CLIENT_ID,
    identityPoolId: config.AWS_COGNITO_IDENTITY_POOL_ID,
    mandatorySignIn: 'false'
  },
  'aws_appsync_graphqlEndpoint': config.API.graphql_endpoint,
  'aws_appsync_region': config.AWS_REGION,
  'aws_appsync_authenticationType': 'AMAZON_COGNITO_USER_POOLS'
});

Amplify.Logger.LOG_LEVEL = 'DEBUG';

function App() {
  const [username, setUsername] = useState('');

  useEffect(() => {
      setUsername(Auth.user.username);
  }, []);

  return (
    <div className="App">
      <NavMenu/>
    </div>
  );
}

export default withAuthenticator(App);
