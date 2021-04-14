import React, { useRef, useState } from 'react';

import { useDispatch, useSelector } from 'react-redux';

import {
    Box, Grid, Text, Button, Tabs, Tab,
} from 'grommet';
import { Menu, Volume, VolumeMute } from 'grommet-icons';
import {
    selectPlayers,
    leave,
    selectActionReference,
} from '../redux/slices/game';

import {
    selectRoomId, mute, unmute, selectMuted,
} from '../redux/slices/hub';

import Player from '../components/Player';
import GameActions from '../GameActions';
import Pot from '../components/Pot';
import RejoinCode from '../components/RejoinCode';
import { useContainerDimensions, useNotifications } from '../utils/hooks';

import Introduction from '../components/information/Introduction';
import HowWePlay from '../components/information/HowWePlay';
import HandRankings from '../components/information/HandRankings';

function Game() {
    const gameAreaRef = useRef(null);
    const players = useSelector(selectPlayers);
    const actionReference = useSelector(selectActionReference);
    const size = useContainerDimensions(gameAreaRef);

    const mobileLayout = size.width < 800 || size.height < 700;

    useNotifications();

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
            name: playerArea, start: [0, index], end: [0, index],
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
    const rows = mobileLayout ? Array(numPlayers + 1).fill('auto') : ['auto', 'auto', 'auto'];

    return (
        <Box fill>
            <Grid
                fill
                areas={[
                    { name: 'gameArea', start: [0, 0], end: [0, 0] },
                    { name: 'actions', start: [0, 1], end: [0, 1] },
                ]}
                columns={['fill']}
                rows={['auto', 'xsmall']}
            >
                <Box gridArea="gameArea" fill pad="small" ref={gameAreaRef}>
                    <Box fill round>
                        <Grid
                            fill
                            areas={gridAreas}
                            columns={columns}
                            rows={rows}
                        >
                            {players.map(({ name, ...player }, position) => (
                                <Box key={name} gridArea={playerAreas[position]}>
                                    <Player name={name} position={position} {...player} />
                                </Box>
                            ))}

                            <Box gridArea="pot" pad={mobileLayout ? 'small' : null}>
                                <Pot />
                            </Box>
                        </Grid>
                    </Box>
                </Box>
                <Box gridArea="actions" direction="column" justify="between" pad="xsmall">
                    <Box><Text size="xsmall">{actionReference}</Text></Box>
                    <Box alignSelf="end"><GameActions /></Box>
                </Box>
            </Grid>
        </Box>
    );
}

function Sidebar() {
    const [activeIndex, setActiveIndex] = useState(2);

    return (
        <Box elevation="small" fill>
            <Tabs onActive={setActiveIndex} activeIndex={activeIndex}>
                <Tab title="Introduction">
                    <Box overflow="auto" fill pad={{ vertical: 'small', horizontal: 'small' }}>
                        <Introduction />
                    </Box>
                </Tab>
                <Tab title="How We Play">
                    <Box overflow="auto" fill pad={{ vertical: 'small', horizontal: 'small' }}>
                        <HowWePlay />
                    </Box>
                </Tab>
                <Tab title="Hand Rankings">
                    <Box overflow="auto" fill pad={{ vertical: 'small', horizontal: 'small' }}>
                        <HandRankings />
                    </Box>
                </Tab>
            </Tabs>
        </Box>
    );
}

function GameContainer({ drawerOpen }) {
    return (
        <Grid
            fill
            areas={[
                { name: 'sidebar', start: [0, 0], end: [0, 0] },
                { name: 'content', start: [1, 0], end: [1, 0] },
            ]}
            columns={[`${drawerOpen ? 400 : 0}px`, 'auto']}
            rows={['fill']}
        >
            <Box gridArea="sidebar">
                {drawerOpen && <Sidebar />}
            </Box>
            <Box gridArea="content">
                <Game />
            </Box>
        </Grid>
    );
}

function Header({ drawerOpen, setDrawerOpen }) {
    const roomId = useSelector(selectRoomId);
    const dispatch = useDispatch();

    const isMuted = useSelector(selectMuted);

    const leaveGame = () => {
        dispatch(leave());
    };

    return (
        <Box pad="small" direction="row" fill justify="between" elevation="small" background="brand">
            <Box direction="row" align="center" gap="small" pad="small">
                <Button plain icon={<Menu onClick={() => setDrawerOpen(!drawerOpen)} />} />
                <Text alignSelf="start" size="xxlarge">{roomId}</Text>
            </Box>
            <Box direction="row" align="center" gap="small" pad="small">
                <Box justify="center">
                    <RejoinCode />
                    <Button onClick={leaveGame}>Leave</Button>
                </Box>
                <Button plain icon={isMuted ? <VolumeMute /> : <Volume />} onClick={() => dispatch(isMuted ? unmute() : mute())} />
            </Box>
        </Box>
    );
}

function MainContainer() {
    const [drawerOpen, setDrawerOpen] = useState(false);

    return (
        <Box style={{ height: '100vh' }}>
            <Grid
                fill
                areas={[
                    { name: 'header', start: [0, 0], end: [0, 0] },
                    { name: 'content', start: [0, 1], end: [0, 1] },
                ]}
                columns={['fill']}
                rows={['xsmall', 'auto']}
            >
                <Box gridArea="header">
                    <Header drawerOpen={drawerOpen} setDrawerOpen={setDrawerOpen} />
                </Box>
                <Box gridArea="content">
                    <GameContainer drawerOpen={drawerOpen} />
                </Box>
            </Grid>
        </Box>
    );
}

export default MainContainer;
