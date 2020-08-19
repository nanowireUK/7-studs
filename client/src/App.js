import React from 'react';
import { useDispatch, useSelector } from 'react-redux';

import { selectGame, join } from './redux/slices/game';

function App() {
  const dispatch = useDispatch();
  const game = useSelector(selectGame);

  const onJoinButtonClicked = () => {
    const gameId = '1';
    const user = 'Robert';

    dispatch(join(gameId, user));
  }

  return (
    <div className="App">
      <button onClick={onJoinButtonClicked}>JOIN</button>
      <pre>{JSON.stringify(game, null, 4)}</pre>
    </div>
  );
}

export default App;
