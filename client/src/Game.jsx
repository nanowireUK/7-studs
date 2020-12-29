import React, { useContext } from 'react';

import { useDispatch, useSelector } from 'react-redux';

import {
    selectPlayers,
    leave,
    selectActionReference,
} from './redux/slices/game';

import { selectRoomId, selectRejoinCode } from './redux/slices/hub';

import Player from './components/Player';
import { Box, Grid, Text, Button, ResponsiveContext } from 'grommet';
import GameActions from './GameActions';
import Pot from './components/Pot';

function Game() {
    const players = useSelector(selectPlayers);
    const roomId = useSelector(selectRoomId);
    const rejoinCode = useSelector(selectRejoinCode);
    const actionReference = useSelector(selectActionReference);
    const dispatch = useDispatch();

    const mobileLayout = useContext(ResponsiveContext) === 'small';

    const leaveGame = () => {
        dispatch(leave());
    }

    // if (inLobby) return <Lobby/>


    const numPlayers = players.length;

    const playerAreas = [
        ['player7'],
        ['player7', 'player2'],
        ['player7', 'player1', 'player3'],
        ['player7', 'player4', 'player2', 'player5'],
        ['player7', 'player4', 'player1', 'player3', 'player5'],
        ['player7', 'player6', 'player1', 'player2', 'player3', 'player8'],
        ['player7', 'player6', 'player4', 'player1', 'player3', 'player5', 'player8'],
        ['player7', 'player6', 'player4', 'player1', 'player2', 'player3', 'player5', 'player8'],
    ][numPlayers - 1];

    const gridAreas = mobileLayout ? [
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

    const columns = mobileLayout ? ['full'] : ['1/3', '1/3', '1/3'];
    const rows = mobileLayout ? Array(numPlayers + 1).fill('auto'): ['auto', 'auto', 'auto'];

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
                    <Text basis="full" alignSelf="center" size="xxlarge">{roomId}</Text>
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
                            <Player name={name} position={position} {...player}></Player>
                        </Box>
                    ))}

                    <Box gridArea="pot" pad={mobileLayout ? 'small' : null}>
                        <Pot />
                    </Box>
                </Grid>
                <Box gridArea="actions" border direction="column" justify="between" pad="xsmall">
                    <Box><Text size="xsmall" color="grey">{actionReference}</Text></Box>
                    <Box alignSelf="end"><GameActions /></Box>
                </Box>
            </Grid>
        </div>
    );
}

export default Game;