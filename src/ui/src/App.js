import React from 'react';
import ReactGA from 'react-ga';
import { NavMenu } from './components/NavMenu';
import './App.css';
import logo from './logo.svg';
ReactGA.initialize('UA-175994834-1');
ReactGA.pageview(window.location.pathname);

function App() {
  return (
    <div className="App">
      <NavMenu/>
      <header className="App-header">
        <img src={logo} className="App-logo" alt="logo" />
      </header>
    </div>
  );
}

export default App;
