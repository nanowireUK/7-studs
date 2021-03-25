import React, { useState } from 'react';

import {
    TextInput, Button, Box, Heading, Text, Card, CardHeader, CardBody, CardFooter,
} from 'grommet';

import { useDispatch, useSelector } from 'react-redux';
import {
    join, rejoin, spectate, selectJoinError, setJoinError,
    selectRoomId, selectUsername,
} from '../redux/slices/hub';

import BackToWelcome from '../components/BackToWelcome';

function Welcome() {
    const [roomId, setRoomId] = useState(useSelector(selectRoomId) || '');
    const [playerName, setPlayerName] = useState(useSelector(selectUsername) || '');
    const [rejoinCode, setRejoinCode] = useState('');
    const joinError = useSelector(selectJoinError);

    const isInvalid = roomId.trim() === '' || playerName.trim() === '' || joinError !== null;

    const dispatch = useDispatch();

    const submitJoin = () => {
        if (!isInvalid) {
            if (rejoinCode.trim() === '') {
                dispatch(join(roomId, playerName));
            } else {
                dispatch(rejoin(roomId, playerName, rejoinCode));
            }
        }
    };

    const submitSpectate = () => {
        if (!isInvalid) {
            if (rejoinCode.trim() === '') {
                dispatch(spectate(roomId, playerName));
            } else {
                dispatch(rejoin(roomId, playerName, rejoinCode));
            }
        }
    };

    const onKeyPress = (e) => {
        if (e.key === 'Enter') submitJoin();
    };

    return (
        <Box background="brand" height="100vh">
            <Card
                justify="center"
                direction="column"
                margin="auto"
            >
                <CardHeader direction="column" gap="xsmall">
                    <BackToWelcome />
                    <Heading fill margin="none" textAlign="center">socialpoker.club</Heading>
                </CardHeader>
                <CardBody direction="column" justify="center" gap="small">
                    <Box direction="row" justify="end">
                        <Text margin="small" alignSelf="center">Room</Text>
                        <Box>
                            <TextInput
                                placeholder="Room Name"
                                value={roomId}
                                onKeyPress={onKeyPress}
                                onChange={(event) => {
                                    setRoomId(event.target.value);
                                    dispatch(setJoinError(null));
                                }}
                            />
                        </Box>
                    </Box>
                    <Box direction="row" justify="end">
                        <Text margin="small" alignSelf="center">Player Name</Text>
                        <Box>
                            <TextInput
                                placeholder="Player Name"
                                value={playerName}
                                onKeyPress={onKeyPress}
                                onChange={(event) => {
                                    setPlayerName(event.target.value);
                                    dispatch(setJoinError(null));
                                }}
                            />
                        </Box>
                    </Box>
                    <Box direction="row" justify="end">
                        <Text margin="small" alignSelf="center">Rejoin Code</Text>
                        <Box>
                            <TextInput
                                placeholder="Rejoin Code (optional)"
                                value={rejoinCode}
                                onKeyPress={onKeyPress}
                                onChange={(event) => {
                                    setRejoinCode(event.target.value);
                                    dispatch(setJoinError(null));
                                }}
                            />
                        </Box>
                    </Box>
                    {joinError ? <Text textAlign="end" color="error" size="small">{joinError}</Text> : null}
                </CardBody>
                <CardFooter direction="row" gap="xsmall" justify="end">
                    <Button primary label="Join" onClick={submitJoin} disabled={isInvalid} />
                    <Button secondary label="Spectate" onClick={submitSpectate} disabled={isInvalid} />
                </CardFooter>
            </Card>
        </Box>
    );
}

export default Welcome;
