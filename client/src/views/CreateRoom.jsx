import React, { useState } from 'react';

import {
    TextInput, Button, Box, Heading, Text, Card, CardHeader, CardBody, CardFooter,
} from 'grommet';

import { useDispatch, useSelector } from 'react-redux';
import {
    create, selectJoinError, setJoinError, selectRoomId, selectUsername,
} from '../redux/slices/hub';

import BackToWelcome from '../components/BackToWelcome';

function Welcome() {
    const [roomId, setRoomId] = useState(useSelector(selectRoomId) || '');
    const [playerName, setPlayerName] = useState(useSelector(selectUsername) || '');
    const joinError = useSelector(selectJoinError);

    const isInvalid = roomId.trim() === '' || playerName.trim() === '' || joinError != null;

    const dispatch = useDispatch();

    const submitCreate = () => {
        if (!isInvalid) {
            dispatch(create(roomId, playerName));
        }
    };

    const onKeyPress = (e) => {
        if (e.key === 'Enter') submitCreate();
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
                    <Box direction="column">
                        <Box direction="row" justify="end">
                            <Text margin="small" alignSelf="center" color={joinError ? 'error' : ''}>Room</Text>
                            <Box>
                                <TextInput
                                    placeholder="Room Name"
                                    value={roomId}
                                    onKeyPress={onKeyPress}
                                    onChange={(event) => {
                                        setRoomId(event.target.value);
                                        dispatch(setJoinError(null));
                                    }}
                                    style={joinError ? { border: '1px solid #f93d20' } : {}}
                                />
                            </Box>
                        </Box>
                        {joinError ? <Text textAlign="end" color="error" size="small">{joinError}</Text> : null}
                    </Box>
                    <Box direction="row" justify="end">
                        <Text margin="small" alignSelf="center">Player Name</Text>
                        <Box>
                            <TextInput
                                placeholder="Player Name"
                                value={playerName}
                                onKeyPress={onKeyPress}
                                onChange={(event) => setPlayerName(event.target.value)}
                            />
                        </Box>
                    </Box>
                </CardBody>
                <CardFooter direction="column" gap="xsmall">
                    <Button link fill primary label="Create" onClick={submitCreate} disabled={isInvalid} />
                </CardFooter>
            </Card>
        </Box>
    );
}

export default Welcome;
