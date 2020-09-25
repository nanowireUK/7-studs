import React, { useState } from 'react';

import { TextInput, Button, Box, Heading } from 'grommet';

import { join, rejoin } from './redux/slices/hub';
import { useDispatch, useSelector } from 'react-redux';
import { selectGameId, selectUsername, selectRejoinCode } from './redux/slices/hub';

function Welcome() {
    const [roomId, setRoomId] = useState(useSelector(selectGameId) || '');
    const [playerName, setPlayerName] = useState(useSelector(selectUsername) || '');
    const [rejoinCode, setRejoinCode] = useState(useSelector(selectRejoinCode) || '');

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
            <TextInput placeholder="Player Name" value={playerName} onChange={event => setPlayerName(event.target.value)} />
                <TextInput 
                placeholder="Room Name" value={roomId} onChange={event => setRoomId(event.target.value)}/>
                
                <TextInput placeholder="Rejoin Code (optional)" value={rejoinCode} onChange={event => setRejoinCode(event.target.value)} />
                <Button primary label="Join" 
                onClick={submitJoin} disabled={isInvalid}/>
        </Box>
    );
}

export default Welcome;
