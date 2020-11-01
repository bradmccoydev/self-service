import React, { Component } from 'react';
import {API} from 'aws-amplify';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableRow from '@material-ui/core/TableRow';
import TableHead from '@material-ui/core/TableHead';
import * as queries from './../graphql/queries';
import * as mutations from './../graphql/mutations';

export default class Home extends Component {
    state = {
        data: [],
    }

    componentDidMount(){
        this.fetch()
    }

    fetch = async () => {
        const applications = await API.graphql({ query: queries.listApplications });
        this.setState({
            data: applications.data.listApplicationMetadataRegistries.items
        });
        console.log(applications.data.listApplicationMetadataRegistries.items)
    }

    render() {
        return (
            <div>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Id</TableCell>
                            <TableCell>Version</TableCell>
                            <TableCell>Title</TableCell>
                            <TableCell>Team</TableCell>
                            <TableCell>Type</TableCell>
                            <TableCell>Endpoint</TableCell>
                            <TableCell>Last Updated By</TableCell> 
                            <TableCell>Last Executed</TableCell>                         
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {this.state.data.map((data) =>
                            <TableRow key={data.id}>
                                <TableCell>{data.id}</TableCell>
                                <TableCell>{data.version}</TableCell>
                                <TableCell>{data.title}</TableCell>
                                <TableCell>{data.team}</TableCell>
                                <TableCell>{data.type}</TableCell>
                                <TableCell>{data.endpoint}</TableCell>
                                <TableCell>{data.last_updated_by}</TableCell>
                                <TableCell>{data.last_executed}</TableCell>

                            </TableRow>
                        )}
                    </TableBody>
                </Table>
            </div>
        );
    }
}