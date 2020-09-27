import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { selectPlayers, selectCanDoAction, start, leave, PlayerActions } from './redux/slices/game';
import { selectUsername, selectGameId } from './redux/slices/hub';
import { Text, Box, Button, Heading } from 'grommet';

function Lobby () {
    const players = useSelector(selectPlayers);
    const gameId = useSelector(selectGameId);
    const username = useSelector(selectUsername);
    const canStart = useSelector(selectCanDoAction(PlayerActions.START));
    const dispatch = useDispatch();

    const startGame = () => {
        dispatch(start());
    }

    const leaveGame = () => {
        dispatch(leave());
    }

    return <Box
            justify="center"
            height="100vh"
            direction="column"
            width="medium"
            gap="small"
            margin="auto">
        <Heading>7 Studs - {gameId}</Heading>
        {players.map(({ name }) => <Text weight={name === username ? 'bold' : 'normal'}>{name}</Text>)}
        <Box direction="row" gap="xsmall">
            {canStart && <Button primary label="Start game"
                onClick={startGame} />}
            <Button label="Leave game"
            onClick={leaveGame} />
        </Box>
    </Box>;
}

export default Lobby;