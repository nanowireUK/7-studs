import React, { useState } from 'react';

import { TextInput, Button, Box, Heading, Text } from 'grommet';

import { join, rejoin } from './redux/slices/hub';
import { useDispatch, useSelector } from 'react-redux';
import { selectRoomId, selectUsername } from './redux/slices/hub';

function Welcome() {
    const [roomId, setRoomId] = useState(useSelector(selectRoomId) || '');
    const [playerName, setPlayerName] = useState(useSelector(selectUsername) || '');
    const [rejoinCode, setRejoinCode] = useState('');

    const isInvalid = roomId.trim() === '' || playerName.trim() === '';

    const dispatch = useDispatch();

    const submitJoin = () => {
        if (rejoinCode.trim() === '') {
            dispatch(join(roomId, playerName));
        } else {
            dispatch(rejoin(roomId, playerName, rejoinCode));
        }
    }

    return (
            <Box
                justify="center"
                height="100vh"
                direction="column"
                width="medium"
                gap="small"
                margin="auto"
            >
                <Heading textAlign="center">Seven Studs</Heading>

                <Box direction="row" justify="end">
                    <Text margin="small" alignSelf="center">Room</Text>
                    <Box><TextInput placeholder="Room Name" value={roomId} onChange={event => setRoomId(event.target.value)}/></Box>
                </Box>
                <Box direction="row" justify="end">
                    <Text margin="small" alignSelf="center">Player Name</Text>
                    <Box><TextInput placeholder="Player Name" value={playerName} onChange={event => setPlayerName(event.target.value)} /></Box>
                </Box>

                <Box direction="row" justify="end">
                    <Text margin="small" alignSelf="center">Rejoin Code</Text>
                    <Box><TextInput placeholder="Rejoin Code (optional)" value={rejoinCode} onChange={event => setRejoinCode(event.target.value)} /></Box>
                </Box>
                <Button primary label="Join" onClick={submitJoin} disabled={isInvalid}/>
        </Box>
    );
}

export default Welcome;
