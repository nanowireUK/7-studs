import React, { useContext } from 'react';

import { useDispatch, useSelector } from 'react-redux';

import {
    selectInLobby,
    selectPlayers,
    selectPots,
    selectGameStatus,
    leave,
} from './redux/slices/game';

import { selectGameId, selectRejoinCode } from './redux/slices/hub';

import Lobby from './Lobby';
import Player from './components/Player';
import { Box, Grid, Text, Button, ResponsiveContext } from 'grommet';
import GameActions from './GameActions';

function Game() {
    const players = useSelector(selectPlayers);
    const inLobby = useSelector(selectInLobby);
    const pots = useSelector(selectPots);
    const gameId = useSelector(selectGameId);
    const rejoinCode = useSelector(selectRejoinCode);
    const gameStatus = useSelector(selectGameStatus);
    const dispatch = useDispatch();

    const size = useContext(ResponsiveContext);

    const leaveGame = () => {
        dispatch(leave());
    }

    if (inLobby) return <Lobby/>

    const numPlayers = players.length;

    const playerAreas = [
        ['player7', 'player2'],
        ['player7', 'player1', 'player3'],
        ['player7', 'player4', 'player2', 'player5'],
        ['player7', 'player4', 'player1', 'player3', 'player5'],
        ['player7', 'player6', 'player1', 'player2', 'player3', 'player8'],
        ['player7', 'player6', 'player4', 'player1', 'player3', 'player5', 'player8'],
        ['player7', 'player6', 'player4', 'player1', 'player2', 'player3', 'player5', 'player8'],
    ][numPlayers - 2];

    const gridAreas = size === 'small' ? [
        ...playerAreas.map((playerArea, index) => ({
            name: playerArea, start: [0, index], end: [0, index]
        })),
        { name: 'pot', start: [0, numPlayers], end: [0, numPlayers] },
    ] : [
        { name: 'player1', start: [0, 0], end: [0, 0] },
        { name: 'player2', start: [1, 0], end: [1, 0] },
        { name: 'player3', start: [2, 0], end: [2, 0] },
        { name: 'player4', start: [0, 1], end: [0, 1] },
        { name: 'pot', start: [1, 1], end: [1, 1] },
        { name: 'player5', start: [2, 1], end: [2, 1] },
        { name: 'player6', start: [0, 2], end: [0, 2] },
        { name: 'player7', start: [1, 2], end: [1, 2] },
        { name: 'player8', start: [2, 2], end: [2, 2] },
    ];

    const columns = size === 'small' ? ['full'] : ['1/3', '1/3', '1/3'];
    const rows = size === 'small' ? ['flex', 'flex', 'flex'] : ['1/3', '1/3', '1/3'];

    return (
        <div style={{ height: '100vh' }}>
            <Grid
                fill={true}
                areas={[
                    { name: 'gameStatus', start: [0, 0], end: [0, 0] },
                    { name: 'gameArea', start: [0, 1], end: [0, 1] },
                    { name: 'actions', start: [0, 2], end: [0, 2] },
                ]}
                columns={['fill']}
                rows={['xsmall', 'auto', 'xsmall']}
            >
                <Box pad="small" gridArea="gameStatus" background="brand" direction="row" fill justify="between">
                    <Text basis="full" alignSelf="center" size="xxlarge">{gameId}</Text>
                    <Box justify="center">
                        <Text alignSelf="center" size="large">{rejoinCode}</Text>
                        <Button color="accent-1" onClick={leaveGame}>Leave</Button>
                    </Box>
                </Box>
                <Grid
                    gridArea="gameArea"
                    fill={true}
                    areas={gridAreas}
                    columns={columns}
                    rows={rows}
                >
                    {players.map(({ name, ...player }, position) => (
                        <Box key={name} gridArea={playerAreas[position]}>
                            <Player name={name} {...player}></Player>
                        </Box>
                    ))}

                    <Box justify="center" align="center" alignContent="center" round={true} gridArea="pot" background="brand" direction="column">
                        {pots.map((pot, index) => (
                            <Text key={index} size="xlarge" textAlign="center">{pot.reduce((a, b) => a + b, 0)}</Text>
                        ))}
                        <Text>{size}</Text>
                    </Box>
                </Grid>
                <Box gridArea="actions" border direction="column" justify="between" pad="xsmall">
                    <Box><Text textAlign="center">{gameStatus}</Text></Box>
                    <Box alignSelf="end"><GameActions /></Box>
                </Box>
            </Grid>
        </div>
    );
}

export default Game;