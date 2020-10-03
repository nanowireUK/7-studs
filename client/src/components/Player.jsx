import React, { useRef, useState, useEffect } from 'react';
import { Box, Text } from 'grommet';
import Casino from 'react-casino';

import {ReactComponent as Heart} from '../assets/suit-hearts.svg';
import {ReactComponent as Club} from '../assets/suit-clubs.svg';
import {ReactComponent as Diamond} from '../assets/suit-diamonds.svg';
import {ReactComponent as Spade} from '../assets/suit-spades.svg';

const useContainerDimensions = myRef => {
    const [dimensions, setDimensions] = useState({ width: 0, height: 0 });

    useEffect(() => {
        const getDimensions = () => ({
            width: myRef.current.offsetWidth,
            height: myRef.current.offsetHeight
        });

        const handleResize = () => {
            setDimensions(getDimensions());
        }

        if (myRef.current) handleResize()

        window.addEventListener("resize", handleResize);

        return () => {
            window.removeEventListener("resize", handleResize);
        }
    }, [myRef]);

    return dimensions;
};

function OldPokerCard ({ face, suit, invisibleToOthers = false, availableDimensions, index }) {
    return (
        <Box title={face !== '?' ? `${face}${suit}` : 'Hidden'}>
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

function Face ({ face = '', suit = '' }) {
    const color = ['H', 'D'].includes(suit.toUpperCase()) ? '#d40000' : 'black';
    return (
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 18 18" version="1.0">
            <text x="50%" y="80%" textAnchor="middle" fontFamily="Bellota Text" fontWeight="700" fill={color}>{face === 'T' ? '10' : face}</text>
        </svg>
    )
}

function Suit ({ suit }) {
    if (suit) {
        switch (suit.toUpperCase()) {
            case 'S': return <Spade/>
            case 'D': return <Diamond/>
            case 'H': return <Heart/>
            case 'C': return <Club/>
        }
    }
    return <Face face="?" suit={suit}/>
}

function PokerCard ({ face, suit, invisibleToOthers = false, availableDimensions, index }) {
    const cardRef = useRef(null);
    const { height } = useContainerDimensions(cardRef);

    if (face === '?' || suit === '?') return (<Box ref={cardRef} direction="row" title={face !== '?' ? `${face}${suit}` : 'Hidden'} elevation="xsmall" pad="xsmall" border round="xsmall" gap="xsmall">
        <Box pad="xsmall" direction="column" align="center" justify="around" width={`${height}px`} testborder={{ color: 'blue', style: 'dashed' }}><Face face={face}/></Box>
    </Box>)

    return (
        <Box ref={cardRef} direction="row" title={face !== '?' ? `${face}${suit}` : 'Hidden'} elevation="xsmall" pad="xsmall" border round="xsmall" gap="xsmall">
            <Box direction="column" align="center" justify="around" width={`${height/2}px`} testborder={{ color: 'blue', style: 'dashed' }}><Face face={face} suit={suit}/></Box>
            <Box direction="column" align="center" justify="around" width={`${height/2}px`} testborder={{ color: 'red', style: 'dashed' }}><Suit suit={suit} /></Box>
        </Box>
    );
}

function Player ({ name, chips, cards, isDealer, isAdmin, isCurrentPlayer, isMe, handDescription, hasFolded, isOutOfThisGame, isSharingHandDetails }) {
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
                <Box margin="xsmall" fill="vertical" border={{ style: "dashed"}} pad="xsmall" round="xsmall">
                    <Box ref={topRef} direction="row" gap="xsmall" fill>
                        {[...cards.slice(0, 2), ...cards.slice(6, 7)].map((card, index) => {
                            const [value, suit] = [...card];

                            return <PokerCard
                                key={index}
                                index={index}
                                face={value}
                                suit={suit}
                                invisibleToOthers={isMe && isSharingHandDetails}
                                availableDimensions={topDimensions}
                            />
                        })}
                    </Box>
                </Box>
                <Box margin="xsmall" fill="vertical" border={{ style: "dashed"}} pad="xsmall" round="xsmall">
                    <Box ref={lowerRef} direction="row" gap="xsmall" fill>
                        {cards.slice(2, 6).map((card, index) => {
                            const [value, suit] = [...card];

                            return <PokerCard
                                key={index}
                                index={index}
                                face={value}
                                suit={suit}
                                availableDimensions={lowerDimensions}
                            />
                        })}
                    </Box>
                </Box>
            </Box>
        </Box>
    );
}

export default Player