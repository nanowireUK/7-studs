import React from 'react';
import ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import { Grommet } from 'grommet';
import store from './redux/store';
import * as serviceWorker from './serviceWorker';


import theme from './theme';

const render = () => {
    const App = require('./App').default

    ReactDOM.render(
        <React.StrictMode>
            <Provider store={store}>
                <Grommet theme={theme}>
                    <App />
                </Grommet>
            </Provider>
        </React.StrictMode>,
        document.getElementById('root')
    );
}

render()

if (process.env.NODE_ENV === 'development' && module.hot) {
    module.hot.accept('./App', render)
}

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
