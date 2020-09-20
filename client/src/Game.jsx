import React, { useState } from 'react';

import { useSelector, useDispatch } from 'react-redux';

import {
    selectInLobby,
    selectPlayers,
    selectPots,
    selectNextAction,
    selectCanDoAction,
    PlayerActions,
    raise,
    fold,
    check,
} from './redux/slices/game';

import Lobby from './Lobby';
import Player from './Player';
import { Box, Grid, Text, Button } from 'grommet';
import GameActions from './GameActions';

function Game() {
    const players = useSelector(selectPlayers);
    const inLobby = useSelector(selectInLobby);
    const pots = useSelector(selectPots);
    const nextAction = useSelector(selectNextAction);

    if (inLobby) return <Lobby/>

    const playerArea = ((numPlayers) => (position) =>
        [['player2', 'player7']][numPlayers][position])(players.length - 2);

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
                <Box gridArea="gameStatus" background="accent-1" />
                <Grid
                    gridArea="gameArea"
                    fill={true}
                    areas={[
                        { name: 'player1', start: [0, 0], end: [0, 0] },
                        { name: 'player2', start: [1, 0], end: [1, 0] },
                        { name: 'player3', start: [2, 0], end: [2, 0] },
                        { name: 'player4', start: [0, 1], end: [0, 1] },
                        { name: 'pot', start: [1, 1], end: [1, 1] },
                        { name: 'player5', start: [2, 1], end: [2, 1] },
                        { name: 'player6', start: [0, 2], end: [0, 2] },
                        { name: 'player7', start: [1, 2], end: [1, 2] },
                        { name: 'player8', start: [2, 2], end: [2, 2] },
                    ]}
                    columns={['1/3', '1/3', '1/3']}
                    rows={['1/3', '1/3', '1/3']}
                >
                    {players.map(({ name, ...player }, position) => (
                        <Box key={name} gridArea={playerArea(position)}>
                            <Player name={name} {...player}></Player>
                        </Box>
                    ))}

                    <Box round={true} gridArea="pot" background="accent-1">
                        <Text>{pots[0].reduce((a, b) => a + b, 0)}</Text>
                    </Box>
                </Grid>
                <Box gridArea="actions" background="accent-2" direction="row">
                    <Text>{nextAction}</Text>
                    <GameActions/>
                </Box>
            </Grid>
        </div>
    );
}

export default Game;