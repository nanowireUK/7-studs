import React, { useRef, useState, useEffect } from 'react';
import { Box, Text } from 'grommet';
import Casino from 'react-casino';

const useContainerDimensions = myRef => {
    const [dimensions, setDimensions] = useState({ width: 0, height: 0 })

    useEffect(() => {
        const getDimensions = () => ({
            width: myRef.current.offsetWidth,
            height: myRef.current.offsetHeight
        })

        const handleResize = () => {
            setDimensions(getDimensions())
        }

        if (myRef.current) handleResize()

        window.addEventListener("resize", handleResize)

        return () => {
            window.removeEventListener("resize", handleResize)
        }
    }, [myRef]);

    return dimensions;
};

function PokerCard ({ face, suit, invisibleToOthers = false, availableDimensions, index }) {
    return (
        <Box key={index} title={face !== '?' ? `${face}${suit}` : 'Hidden'}>
            <Casino.Card
                face={face}
                suit={suit}
                style={{
                    maxWidth: availableDimensions.width / 4.75,
                    maxHeight: availableDimensions.height,
                    height: undefined,
                    width: undefined,
                    marginLeft: index ? '4px' : null,
                    filter: invisibleToOthers ? 'opacity(70%)' : 'none'
                }}
            />
        </Box>
    );
}

function Player ({ name, chips, cards, isDealer, isAdmin, isCurrentPlayer, isMe, handDescription, hasFolded, isOutOfThisGame }) {
    const topRef = useRef(null);
    const lowerRef = useRef(null);

    const topDimensions = useContainerDimensions(topRef);
    const lowerDimensions = useContainerDimensions(lowerRef);

    let status;
    if (hasFolded) status = 'Folded';
    if (isOutOfThisGame) status = 'Out';

    return (
        <Box pad="small" fill >
            <Box pad="xsmall" round={true} fill elevation={isCurrentPlayer ? 'medium': 'small'} border={isCurrentPlayer ? { color: 'accent-1', size: 'medium' } : { color: 'white', size: 'medium' }}>
                <Text size="xlarge" color={isMe ? 'brand' : null}>{name} ({chips} chips)</Text>
                <Text level={3} color='gray'>{handDescription}</Text>
                <Box margin="xsmall" ref={topRef} direction="row" fill>
                    {[...cards.slice(0, 2), ...cards.slice(6, 7)].map((card, index) => {
                        const [value, suit] = [...card];

                        return <PokerCard
                            index={index}
                            face={value}
                            suit={suit}
                            invisibleToOthers={isMe}
                            availableDimensions={topDimensions}
                        />
                    })}
                </Box>
                <Box margin="xsmall" ref={lowerRef} direction="row" fill>
                    {cards.slice(2, 6).map((card, index) => {
                        const [value, suit] = [...card];

                        return <PokerCard
                            index={index}
                            face={value}
                            suit={suit}
                            availableDimensions={lowerDimensions}
                        />
                    })}
                </Box>
            </Box>
        </Box>
    );
}

export default Player