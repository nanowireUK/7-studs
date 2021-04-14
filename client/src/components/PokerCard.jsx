import React from 'react';

import { Box } from 'grommet';

import { ReactComponent as Heart } from '../assets/images/suit-hearts.svg';
import { ReactComponent as Club } from '../assets/images/suit-clubs.svg';
import { ReactComponent as Diamond } from '../assets/images/suit-diamonds.svg';
import { ReactComponent as Spade } from '../assets/images/suit-spades.svg';
import CardBack from '../assets/images/card-back.svg';

function generateTitle(suit, face) {
    const suits = {
        D: 'Diamonds',
        C: 'Clubs',
        S: 'Spades',
        H: 'Hearts',
    };
    const faces = {
        A: 'Ace',
        1: 'One',
        2: 'Two',
        3: 'Three',
        4: 'Four',
        5: 'Five',
        6: 'Six',
        7: 'Seven',
        8: 'Eight',
        9: 'Nine',
        T: 'Ten',
        J: 'Jack',
        Q: 'Queen',
        K: 'King',
    };
    return `${faces[face.toUpperCase()]} of ${suits[suit.toUpperCase()]}`;
}

function Face({ face = '', suit = '', opacity = null }) {
    const color = ['H', 'D'].includes(suit.toUpperCase()) ? '#d40000' : 'black';
    return (
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 18 18" version="1.0">
            <text x="50%" y="80%" textAnchor="middle" fontFamily="Bellota Text" fontWeight="700" fill={color} opacity={opacity}>{face === 'T' ? '10' : face}</text>
        </svg>
    );
}

function Suit({ suit, opacity = null }) {
    if (suit) {
        switch (suit.toUpperCase()) {
        case 'S': return <Spade opacity={opacity} />;
        case 'D': return <Diamond opacity={opacity} />;
        case 'H': return <Heart opacity={opacity} />;
        case 'C': return <Club opacity={opacity} />;
        default: break;
        }
    }
    return <Face opacity={opacity} face="?" suit={suit} />;
}

// const AnimatedBox = animated(Box);

export default function PokerCard({
    face, suit, invisibleToOthers = false, cardIndex = -1, size,
}) {
    const hiddenCard = face === '?' || suit === '?';

    return (
        <Box
            flex={false}
            style={{
                height: `${size}px`,
                width: `${size}px`,
            }}
            direction="row"
            elevation="xsmall"
            round="xsmall"
            border
            title={hiddenCard ? 'Hidden' : generateTitle(suit, face)}
            background={hiddenCard ? {
                image: `url(${CardBack})`,
                size: '115%',
            } : 'none'}
            gap={face==='T' ? 'xsmall' : 'none'}
            pad={size <= 50 ? 'xxsmall' : 'xsmall'}
        >
            {!hiddenCard && <Face opacity={invisibleToOthers ? '50%' : null} face={face} suit={suit} />}
            {!hiddenCard && <Suit opacity={invisibleToOthers ? '50%' : null} face={face} suit={suit} />}
        </Box>
    );
}






    /*
    const props = useSpring({
        delay: cardIndex * 200,
        immediate: cardIndex === -1,
        transform: 'perspective(300px) rotateY(0deg)',
        from: { transform: 'perspective(300px) rotateY(90deg)' }
    });

    return <AnimatedBox style={props} fill="vertical" ref={cardRef}>
        {face === '?' || suit === '?' ? (
            <Box fill="vertical" direction="row" title="Hidden" elevation="xsmall" pad="xsmall" border round="xsmall" gap="xsmall" background={{
                image: `url(${CardBack})`,
                size: "115%"
            }}>
                <Box pad="xsmall" direction="column" align="center" justify="around" width={`${height + 2}px`} testborder={{ color: 'blue', style: 'dashed' }}></Box>
            </Box>
        ) : (
            <Box fill="vertical" direction="row" title={generateTitle(suit, face)} elevation="xsmall" pad="xsmall" border round="xsmall" gap="2px" background="white">
                <Box direction="column" align="center" justify="around" width={`${height/2}px`} testborder={{ color: 'blue', style: 'dashed' }}><Face invisibleToOthers={invisibleToOthers} face={face} suit={suit}/></Box>
                <Box direction="column" align="center" justify="around" width={`${height/2}px`} testborder={{ color: 'red', style: 'dashed' }}><Suit invisibleToOthers={invisibleToOthers} suit={suit} /></Box>
            </Box>
        )}
    </AnimatedBox> */