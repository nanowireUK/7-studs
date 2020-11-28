import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { selectPlayers, selectCanDoAction, start, proceed, leave, PlayerActions, selectCurrentGameStandings, selectPreviousGameResults } from './redux/slices/game';
import { selectUsername, selectRoomId, selectRejoinCode } from './redux/slices/hub';
import { Text, Box, Button, Heading, Grid } from 'grommet';

function ordinal(i) {
    const j = i % 10, k = i % 100;
    if (j === 1 && k !== 11) return i + "st";
    if (j === 2 && k !== 12) return i + "nd";
    if (j === 3 && k !== 13) return i + "rd";
    return i + "th";
}

function Lobby () {
    const players = useSelector(selectPlayers);
    const roomId = useSelector(selectRoomId);
    const rejoinCode = useSelector(selectRejoinCode);
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

    return <div style={{ height: '100vh' }}>
        <Grid
            fill={true}
            areas={[
                { name: 'header', start: [0, 0], end: [0, 0] },
                { name: 'lobby', start: [0, 1], end: [0, 1] },
                { name: 'footer', start: [0, 2], end: [0, 2] },
            ]}
            columns={['fill']}
            rows={['xsmall', 'auto', 'xsmall']}
        >
            <Box pad="small" gridArea="header" direction="column">
                <Box margin="small" alignSelf="end" alignContent="center" fill="vertical" justify="center">
                    <Text alignSelf="center" size="large">{rejoinCode}</Text>
                    <Button color="accent-1" onClick={leaveGame}>Leave</Button>
                </Box>
            </Box>
            <Box
            justify="center"
            direction="column"
            width="500px"
            gap="small"
            margin="auto"
            gridArea="lobby">
                <Heading>7 Studs - {roomId}</Heading>
                {playerList}
                <Box direction="row" gap="xsmall" wrap>
                    {canStart && <Button margin="xxsmall" primary label="Start new game"
                        onClick={startGame} />}
                    {canContinue && <Button margin="xxsmall" primary label="Continue game"
                        onClick={continueGame} />}
                </Box>
                {previousGameResults ? (
                    <Box>
                        <Heading level={2}>Previous Games</Heading>
                        {previousGameResults.map((result, index) => <Text key={index}>{result}</Text>)}
                    </Box>
                ) : null}
            </Box>
        </Grid>
    </div>;
}

export default Lobby;