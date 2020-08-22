import React from 'react';
import { Grommet } from 'grommet';
import { useSelector } from 'react-redux';

import { selectGame } from './redux/slices/game';
import theme from './theme';

import Game from './Game';
import Welcome from './Welcome';

function App() {
    const game = useSelector(selectGame);

    return <Grommet theme={theme}>
        {game === null ? <Welcome /> : <Game />}
    </Grommet>;
}

export default App;
