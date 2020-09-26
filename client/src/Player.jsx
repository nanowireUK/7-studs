import React from 'react';
import { Box, Heading } from 'grommet';
import { Card } from 'react-casino';

function Player ({ name, chips, cards, isDealer, isAdmin, isCurrentPlayer, isMe }) {
    return (
        <Box margin="small" round={true} align="center" border={isCurrentPlayer}>
            <Heading level={3} color={isMe ? 'brand' : null}> {name} ({chips} chips)</Heading>
            <Box direction="row">
                {cards.map((card, index) => {
                    const [value, suit] = [...card];

                    return (
                        <Card
                            key={index}
                            face={value}
                            suit={suit}
                            style={{
                                width: undefined,
                                height: '12vh',
                                marginLeft: index ? '4px' : null
                            }}
                        />
                    );
                })}
            </Box>
        </Box>
    );
}

export default Player