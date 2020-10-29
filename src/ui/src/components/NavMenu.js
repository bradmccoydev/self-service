import React, { Component } from 'react';
import { makeStyles } from '@material-ui/core/styles';
import { Link } from 'react-router-dom';
import IconButton from '@material-ui/core/IconButton';
import AppBar from '@material-ui/core/AppBar';
import Toolbar from '@material-ui/core/Toolbar';
import Box from '@material-ui/core/Box';
import github from '.././images/github.png'
import './NavMenu.css';

const classes = makeStyles((theme) => ({
  header: {
    marginRight: 1,
    marginLeft: 1,
    margin: "10px",
    },
  iconStyles: {
    marginRight: 0,
    },
  toolbarButtons: {
    marginLeft: 'auto',
    display: "initial",
    textAlign: "bottom"
  },
}));

export class NavMenu extends Component {
  static displayName = NavMenu.name;  

  constructor (props) {
    super(props);

    this.toggleNavbar = this.toggleNavbar.bind(this);
    this.state = {
      collapsed: true
    };
  }

  toggleNavbar () {
    this.setState({
      collapsed: !this.state.collapsed
    });
  }

  render () {
    return (
      <div>
      <AppBar position="static" style={{ background: 'transparent', boxShadow: 'none'}}>
        <Toolbar className="nav-bar">
          <Box 
            display='flex'
            flexGrow={1}
            style={{ textDecoration: 'none' }}
            component={Link} to="/home/showheader=true">
            SELF SERVICE
          </Box>
          <IconButton
            style={{gutterBottom: true}}
            color="inherit"
            aria-label="Menu"
            onClick={event =>  window.location.href='https://github.com/bradmccoydev/self-service'}>                                 
            <img
              style={classes.iconStyles}
              src={github}
              alt="Dojo"
              width="30"
              height="30" />                      
          </IconButton>
        </Toolbar>
        </AppBar>
      </div>
      )}               
  }