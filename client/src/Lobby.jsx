import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { selectPlayers, selectCanDoAction, start, proceed, leave, PlayerActions, selectCurrentGameStandings, selectPreviousGameResults } from './redux/slices/game';
import { selectUsername, selectGameId } from './redux/slices/hub';
import { Text, Box, Button, Heading } from 'grommet';

function ordinal(i) {
    const j = i % 10, k = i % 100;
    if (j === 1 && k !== 11) return i + "st";
    if (j === 2 && k !== 12) return i + "nd";
    if (j === 3 && k !== 13) return i + "rd";
    return i + "th";
}

function Lobby () {
    const players = useSelector(selectPlayers);
    const gameId = useSelector(selectGameId);
    const username = useSelector(selectUsername);
    const canStart = useSelector(selectCanDoAction(PlayerActions.START));
    const canContinue = useSelector(selectCanDoAction(PlayerActions.CONINUE));

    const currentGameStandings = useSelector(selectCurrentGameStandings);
    const previousGameResults = useSelector(selectPreviousGameResults);

    const dispatch = useDispatch();

    const startGame = () => {
        dispatch(start());
    }

    const continueGame = () => {
        dispatch(proceed());
    }

    const leaveGame = () => {
        dispatch(leave());
    }

    const playerList = currentGameStandings.length ?
        currentGameStandings.map(({ PlayerName, RemainingFunds }, position) => (
            <Box key={PlayerName}><Text weight={PlayerName === username ? 'bold' : 'normal'}>{ordinal(position + 1)}: {PlayerName} ({RemainingFunds})</Text></Box>
        )) : players.map(({ name }) => <Text key={name} weight={name === username ? 'bold' : 'normal'}>{name}</Text>);


    return <Box
            justify="center"
            height="100vh"
            direction="column"
            width="500px"
            gap="small"
            margin="auto">
        <Heading>7 Studs - {gameId}</Heading>
        {playerList}
        <Box direction="row" gap="xsmall" wrap>
            {canStart && <Button margin="xxsmall" primary label="Start new game"
                onClick={startGame} />}
            {canContinue && <Button margin="xxsmall" primary label="Continue game"
                onClick={continueGame} />}
            <Button margin="xxsmall" label="Leave game"
            onClick={leaveGame} />
        </Box>
        {previousGameResults ? (
            <Box>
                <Heading level={2}>Previous Games</Heading>
                {previousGameResults.map(result => <Text>{result}</Text>)}
            </Box>
        ) : null}
    </Box>;
}

export default Lobby;