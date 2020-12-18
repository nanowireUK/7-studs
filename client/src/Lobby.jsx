import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { selectPlayers, selectCanDoAction, start, proceed, leave, PlayerActions, selectCurrentGameStandings, selectIsAdmin, selectAdminName } from './redux/slices/game';
import { selectUsername, selectRoomId, selectRejoinCode } from './redux/slices/hub';
import { Text, Box, Button, Heading, Grid } from 'grommet';

function ordinal(i) {
    const j = i % 10, k = i % 100;
    if (j === 1 && k !== 11) return i + "st";
    if (j === 2 && k !== 12) return i + "nd";
    if (j === 3 && k !== 13) return i + "rd";
    return i + "th";
}

function Player ({ name, hasLeftRoom, remainingFunds, status, position }) {
    const username = useSelector(selectUsername);
    const isMe = username === name;

    const isSpectator = status === 'Spectator';
    const inMostRecentGame = status === 'PartOfMostRecentGame';
    const textColor = hasLeftRoom ? 'gray' : 'black';

        return <Box
            direction="row"
            gap="xsmall"
        >
            <Text color={textColor}>{inMostRecentGame ? `${ordinal(position + 1)}:` : '-'}</Text>
            <Text color={textColor} weight={isMe ? 'bold' : 'normal'}>{name}</Text>
            <Text color={textColor}>
                {(() => {
                    if (inMostRecentGame) return `(${remainingFunds})`;
                    else if (isSpectator) return '(Spectating)';
                    return '';
                })()}
            </Text>
        </Box>
}

function Lobby () {
    const players = useSelector(selectPlayers);
    const roomId = useSelector(selectRoomId);
    const rejoinCode = useSelector(selectRejoinCode);

    const username = useSelector(selectUsername);
    const canStart = useSelector(selectCanDoAction(PlayerActions.START));
    const canContinue = useSelector(selectCanDoAction(PlayerActions.CONINUE));
    const isAdmin = useSelector(selectIsAdmin);
    const adminName = useSelector(selectAdminName);

    const currentGameStandings = useSelector(selectCurrentGameStandings);

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
        currentGameStandings.map(({ name, ...player }, position) => (
            <Player key={name} name={name} {...player} position={position} />
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
            <Box pad="small" gridArea="header" direction="column" 
            background="brand">
                <Box margin="small" alignSelf="end" alignContent="center" fill="vertical" justify="center">
                    <Text alignSelf="center" size="large">{rejoinCode}</Text>
                    <Button color="accent-1" onClick={leaveGame}>Leave</Button>
                </Box>
            </Box>
            <Box
                pad="small"
                justify="center"
                direction="column"
                width="500px"
                gap="small"
                margin="auto"
                gridArea="lobby"
            >
                <Box
                    justify="center"
                    border={true}
                    margin="xsmall"
                    pad="small"
                    round="small"
                >
                    <Heading margin="small">7 Studs - {roomId}</Heading>
                    <Box pad="medium">
                        {playerList}
                    </Box>
                    <Box direction="row" gap="xsmall" justify="end">
                        {!isAdmin && <Text>Waiting for {adminName} to start a game</Text>}
                        {canStart && <Button margin="xxsmall" primary label="Start new game"
                            onClick={startGame} />}
                        {canContinue && <Button margin="xxsmall" primary label="Continue game"
                            onClick={continueGame} />}
                    </Box>
                </Box>
            </Box>
        </Grid>
    </div>;
}

export default Lobby;