import React, { Component } from 'react';
import {API} from 'aws-amplify';
import * as queries from './../graphql/queries';
import * as mutations from './../graphql/mutations';

export default class Home extends Component {

    componentDidMount(){
        this.fetch()
    }

    fetch = async () => {
        const applications = await API.graphql({ query: queries.listApplications });
        console.log(applications)
    }

    render() {
        return (
            <div>
                HEllo!
            </div>
        );
    }
}