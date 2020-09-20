import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { selectPlayers, selectCanDoAction, start, PlayerActions } from './redux/slices/game';
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

    return <Box>
        <Heading>{gameId}</Heading>
        {players.map(({ name }) => <Text weight={name === username ? 'bold' : 'normal'}>{name}</Text>)}
        <Button primary label="Start game"
            onClick={startGame} disabled={!canStart} />
    </Box>;
}

export default Lobby;