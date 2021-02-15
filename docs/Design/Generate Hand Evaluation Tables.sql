--
-- Generate hand evaluation tables
--

-------------------------------------------------------------------------------------------
-- (1) Set up ranks, suits and cards

IF object_id('tempdb..#ranks') IS NOT NULL DROP TABLE #ranks

create table #ranks (
	rank_code varchar(2),
	rank_name varchar(10),
	rank_name_plural varchar(10),
	rank_prime int,
	pos int
)

insert #ranks values('2', 'Two', 'Twos', 2, 2)
insert #ranks values('3', 'Three', 'Threes', 3, 3)
insert #ranks values('4', 'Four', 'Fours', 5, 4)
insert #ranks values('5', 'Five', 'Fives', 7, 5)
insert #ranks values('6', 'Six', 'Sixes', 11, 6)
insert #ranks values('7', 'Seven', 'Sevens', 13, 7)
insert #ranks values('8', 'Eight', 'Eights', 17, 8)
insert #ranks values('9', 'Nine', 'Nines', 19, 9)
insert #ranks values('T', 'Ten', 'Tens', 23, 10)
insert #ranks values('J', 'Jack', 'Jacks', 29, 11)
insert #ranks values('Q', 'Queen', 'Queens', 31, 12)
insert #ranks values('K', 'King', 'Kings', 37, 13)
insert #ranks values('A', 'Ace', 'Aces', 41, 14)

IF object_id('tempdb..#suits') IS NOT NULL DROP TABLE #suits

create table #suits (
	suit_code char(1),
	suit_name char(10)
)

insert #suits values('s', 'Spades')
insert #suits values('h', 'Hearts')
insert #suits values('c', 'Clubs')
insert #suits values('d', 'Diamonds')

IF object_id('tempdb..#cards') IS NOT NULL DROP TABLE #cards

select c.*, s.*, c.rank_name + ' of ' + s.suit_name as card_text, c.rank_code + s.suit_code as card_code
into #cards
from #suits s
cross join #ranks c

select * from #cards order by suit_code desc, rank_prime

-------------------------------------------------------------------------------------------
-- (2) Set up relative rankings of combinations of 1 card (simplest case, i.e. simple card value wins)

IF object_id('tempdb..#combos1') IS NOT NULL DROP TABLE #combos1

SELECT d.*, RANK() OVER(ORDER BY hand_value DESC) AS eval_rank
INTO #combos1
FROM (
	SELECT DISTINCT 
		rank_prime AS hand_signature, 
		rank_name AS hand_name, 
		rank_prime AS hand_value 
	FROM #cards
) d

SELECT * FROM #combos1 ORDER BY eval_rank 

select 'EvalHands.Add('+LTRIM(STR(hand_signature))+', new EvalHand('+LTRIM(STR(hand_signature))	+', '+LTRIM(STR(eval_rank))	+', "'+hand_name+'"));' as new_eval_stmt
from #combos1 
ORDER BY eval_rank 

-- EvalHands.Add(-25911877, new EvalHand(-25911877, 323, "Ace-High Flush"));

-------------------------------------------------------------------------------------------
-- (3) Set up relative rankings of combinations of 2 cards (pairs or highs only)

IF object_id('tempdb..#combos2') IS NOT NULL DROP TABLE #combos2

SELECT d.*, RANK() OVER(ORDER BY hand_value DESC) AS eval_rank
INTO #combos2
FROM (
	SELECT DISTINCT 
		c1.rank_prime * c2.rank_prime AS hand_signature, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime THEN 'Pair of ' + c1.rank_name_plural
			ELSE c1.rank_name +' High with '+c2.rank_name
		END 
			AS hand_name, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime THEN 200000 + ( c1.rank_prime * 100 )
			ELSE 100000 + ( c1.rank_prime * 100 ) + ( c2.rank_prime )
		END 
			AS hand_value 
	FROM #cards c1
	CROSS JOIN #cards c2
	WHERE c1.card_code <> c2.card_code -- don't allow a card to be combined with itself
	AND c1.rank_prime >= c2.rank_prime -- consider only the combinations where the cards are in order from left to right
) d

SELECT * FROM #combos2 ORDER BY eval_rank 

select 'EvalHands.Add('+LTRIM(STR(hand_signature))+', new EvalHand('+LTRIM(STR(hand_signature))	+', '+LTRIM(STR(eval_rank))	+', "'+hand_name+'"));' as new_eval_stmt
from #combos2 
ORDER BY eval_rank 

-- EvalHands.Add(-25911877, new EvalHand(-25911877, 323, "Ace-High Flush"));

-------------------------------------------------------------------------------------------
-- (4) Set up relative rankings of combinations of 3 cards (three of a kind, pairs or highs only)

IF object_id('tempdb..#combos3') IS NOT NULL DROP TABLE #combos3

SELECT d.*, RANK() OVER(ORDER BY hand_value DESC) AS eval_rank
INTO #combos3
FROM (
	SELECT DISTINCT 
		c1.rank_prime * c2.rank_prime * c3.rank_prime  AS hand_signature, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime THEN 'Three ' + c1.rank_name_plural
			WHEN c1.rank_prime = c2.rank_prime THEN 'Pair of ' + c1.rank_name_plural + ' with ' +c3.rank_name
			WHEN c2.rank_prime = c3.rank_prime THEN 'Pair of ' + c2.rank_name_plural + ' with ' +c1.rank_name
			ELSE c1.rank_name +' High with '+c2.rank_name+', '+c3.rank_name
		END 
			AS hand_name, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime THEN 30000000 + ( c1.rank_prime * 10000 )
			WHEN c1.rank_prime = c2.rank_prime THEN 20000000 + ( c1.rank_prime * 10000 ) + ( c3.rank_prime ) 
			WHEN c2.rank_prime = c3.rank_prime THEN 20000000 + ( c2.rank_prime * 10000 ) + ( c1.rank_prime ) 
			ELSE 10000000 + ( c1.rank_prime * 10000 ) + ( c2.rank_prime * 100) + ( c3.rank_prime ) 
		END 
			AS hand_value 
	FROM #cards c1
	CROSS JOIN #cards c2
	CROSS JOIN #cards c3
	WHERE ( c1.card_code <> c2.card_code AND c1.card_code <> c3.card_code AND c2.card_code <> c3.card_code ) -- don't allow a card to be combined with itself
	AND ( c1.rank_prime >= c2.rank_prime AND c2.rank_prime >= c3.rank_prime ) -- consider only the combinations where the cards are in order from left to right
) d

SELECT * FROM #combos3 ORDER BY eval_rank 

select 'EvalHands.Add('+LTRIM(STR(hand_signature))+', new EvalHand('+LTRIM(STR(hand_signature))	+', '+LTRIM(STR(eval_rank))	+', "'+hand_name+'"));' as new_eval_stmt
from #combos3 
ORDER BY eval_rank 

-- EvalHands.Add(-25911877, new EvalHand(-25911877, 323, "Ace-High Flush"));

-------------------------------------------------------------------------------------------
-- (4) Set up relative rankings of combinations of 4 cards (four of a kind, three of a kind, two pairs, pairs or highs only)

IF object_id('tempdb..#combos4') IS NOT NULL DROP TABLE #combos4

SELECT d.*, RANK() OVER(ORDER BY hand_value DESC) AS eval_rank
INTO #combos4
FROM (
	SELECT DISTINCT 
		c1.rank_prime * c2.rank_prime * c3.rank_prime * c4.rank_prime  AS hand_signature, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime AND c3.rank_prime = c4.rank_prime THEN 'Four ' + c1.rank_name_plural
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime THEN 'Three ' + c1.rank_name_plural
			WHEN c2.rank_prime = c3.rank_prime AND c3.rank_prime = c4.rank_prime THEN 'Three ' + c2.rank_name_plural
			WHEN c1.rank_prime = c2.rank_prime AND c3.rank_prime = c4.rank_prime THEN 'Two Pair, ' + c1.rank_name_plural + ' over ' + c3.rank_name_plural
			WHEN c1.rank_prime = c2.rank_prime THEN 'Pair of ' + c1.rank_name_plural + ' with ' + c3.rank_name + ', ' + c4.rank_name
			WHEN c2.rank_prime = c3.rank_prime THEN 'Pair of ' + c2.rank_name_plural + ' with ' + c1.rank_name + ', ' + c4.rank_name
			WHEN c3.rank_prime = c4.rank_prime THEN 'Pair of ' + c3.rank_name_plural + ' with ' + c1.rank_name + ', ' + c2.rank_name
			ELSE c1.rank_name +' High with '+c2.rank_name+', '+c3.rank_name+', '+c4.rank_name
		END 
			AS hand_name, 
		CASE 
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime AND c3.rank_prime = c4.rank_prime THEN 50000000000 + ( c1.rank_prime * 1000000 ) -- 4 of a kind
			WHEN c1.rank_prime = c2.rank_prime AND c2.rank_prime = c3.rank_prime THEN 40000000000 + ( c1.rank_prime * 1000000 ) -- 3
			WHEN c2.rank_prime = c3.rank_prime AND c3.rank_prime = c4.rank_prime THEN 40000000000 + ( c2.rank_prime * 1000000 ) -- 3
			WHEN c1.rank_prime = c2.rank_prime AND c3.rank_prime = c4.rank_prime THEN 30000000000 + ( c1.rank_prime * 1000000 ) + ( c3.rank_prime * 10000 ) -- 2 pair
			WHEN c1.rank_prime = c2.rank_prime THEN 20000000000 + ( c1.rank_prime * 1000000 )+ ( c3.rank_prime * 10000 ) + ( c4.rank_prime * 100 ) 
			WHEN c2.rank_prime = c3.rank_prime THEN 20000000000 + ( c2.rank_prime * 1000000 )+ ( c1.rank_prime * 10000 ) + ( c4.rank_prime * 100) 
		  --WHEN c3.rank_prime = c4.rank_prime THEN 20000000000 + ( c2.rank_prime * 1000000 )+ ( c1.rank_prime * 10000 ) + ( c2.rank_prime * 100) ---- Bug fixed 2020-12-30 (should have used c3 not c2)
			WHEN c3.rank_prime = c4.rank_prime THEN 20000000000 + ( c3.rank_prime * 1000000 )+ ( c1.rank_prime * 10000 ) + ( c2.rank_prime * 100) 
			ELSE 10000000000 + ( c1.rank_prime * 1000000 ) + ( c2.rank_prime * 10000) + ( c3.rank_prime * 100) + ( c4.rank_prime )
			-- Note re bug fix: The rank for a pair was being calculated wrongly where:
			--   (a) the visible hand being considered consists of four cards and
			--   (b) the hand is a pair and 
			--   (c) the paired card is less than both the other visible cards
			-- And it would only affect play when:
			--   (a) there is a better visible pair on the table, and 
			--   (b) there is nothing visible that is better than a pair, and
			--   (c) the second highest card in the problematic visible hand is greater than or equal to the cards that are paired in the better hand
			-- Even the other cards in both hands make a difference. 
			-- This hand from game 3 on 29/12/20 had all of it:
			--   John: 9-9-2-4 -> 9-9-4-2
			--   Fab:  J-3-9-3 -> J-9-3-3 (was wrongly getting higher ranking due to bug highlighted above)
		END 
			AS hand_value 
	FROM #cards c1
	CROSS JOIN #cards c2
	CROSS JOIN #cards c3
	CROSS JOIN #cards c4
	WHERE ( 
		c1.card_code <> c2.card_code AND
		c1.card_code <> c3.card_code AND 
		c1.card_code <> c4.card_code AND 
		c2.card_code <> c3.card_code AND
		c2.card_code <> c4.card_code AND
		c3.card_code <> c4.card_code 
	) -- don't allow a card to be combined with itself
	AND ( 
		c1.rank_prime >= c2.rank_prime AND
		c2.rank_prime >= c3.rank_prime AND
		c3.rank_prime >= c4.rank_prime 
	) -- consider only the combinations where the cards are in order from left to right
) d

SELECT * FROM #combos4 ORDER BY hand_value DESC -- High hand values is better hand 
SELECT * FROM #combos4 ORDER BY eval_rank ASC -- Low rank is a better hand

select 'EvalHands.Add('+LTRIM(STR(hand_signature))+', new EvalHand('+LTRIM(STR(hand_signature))	+', '+LTRIM(STR(eval_rank))	+', "'+hand_name+'"));' as new_eval_stmt
from #combos4 
ORDER BY eval_rank 

-- EvalHands.Add(-25911877, new EvalHand(-25911877, 323, "Ace-High Flush"));

-------------------------------------------------------------------------------------------
-- (5) Set up relative rankings of combinations of 5 cards (i.e. full set of valid poker hands)

IF object_id('tempdb..#combos5') IS NOT NULL DROP TABLE #combos5

SELECT d.*, RANK() OVER(ORDER BY hand_value DESC) AS eval_rank
INTO #combos5
--SELECT COUNT(*)
FROM (
	SELECT DISTINCT 
		(c1.rank_prime * c2.rank_prime * c3.rank_prime * c4.rank_prime * c5.rank_prime)
			* CASE WHEN ( c1.suit_code = c2.suit_code AND c2.suit_code = c3.suit_code AND c3.suit_code = c4.suit_code AND c4.suit_code = c5.suit_code ) THEN -1 ELSE 1 END
			AS hand_signature, -- Note that this signature works regardless of card order. Straight flushes and flushes get a minus number to distinguish them from non-flush equivalents (straights and highs)
		CASE
			-- Recognise hands from the patterns that the cards form when sorted high-to-low (L-R).
			-- So pairs etc. (AA, AAA, AAAA) will always be grouped, but countback cards (x) could be higher or lower than the paired cards

			-- Four of a kind (AAAAx or xAAAA)
			WHEN c1.pos = c2.pos AND c2.pos = c3.pos AND c3.pos = c4.pos THEN 'Four ' + c1.rank_name_plural -- AAAAx
			WHEN c2.pos = c3.pos AND c3.pos = c4.pos AND c4.pos = c5.pos THEN 'Four ' + c2.rank_name_plural -- xAAAA
			-- Full house (AAABB or BBAAA)
			WHEN c1.pos = c2.pos AND c2.pos = c3.pos AND c4.pos = c5.pos THEN c1.rank_name_plural + ' Full over ' + c4.rank_name_plural -- AAABB
			WHEN c1.pos = c2.pos AND c3.pos = c4.pos AND c4.pos = c5.pos THEN c3.rank_name_plural + ' Full over ' + c1.rank_name_plural -- BBAAA
			-- Three of a kind (AAAxx or xAAAx or xxAAA)
			WHEN c1.pos = c2.pos AND c2.pos = c3.pos THEN 'Three ' + c1.rank_name_plural -- AAAxx
			WHEN c2.pos = c3.pos AND c3.pos = c4.pos THEN 'Three ' + c2.rank_name_plural -- xAAAx
			WHEN c3.pos = c4.pos AND c4.pos = c5.pos THEN 'Three ' + c3.rank_name_plural -- xxAAA
			-- Two pair (AABBx or AAxBB or xAABB)
			WHEN c1.pos = c2.pos AND c3.pos = c4.pos THEN c1.rank_name_plural + ' and ' + c3.rank_name_plural -- AABBx
			WHEN c1.pos = c2.pos AND c4.pos = c5.pos THEN c1.rank_name_plural + ' and ' + c4.rank_name_plural -- AAxBB
			WHEN c2.pos = c3.pos AND c4.pos = c5.pos THEN c2.rank_name_plural + ' and ' + c4.rank_name_plural -- xAABB
			-- Pair (AAxxx or xAAxx or xxAAx or xxxAA)
			WHEN c1.pos = c2.pos THEN 'Pair of ' + c1.rank_name_plural /*+ ' with ' + c3.rank_name + ', ' + c4.rank_name + ', ' + c5.rank_name */ -- AAxxx
			WHEN c2.pos = c3.pos THEN 'Pair of ' + c2.rank_name_plural /*+ ' with ' + c1.rank_name + ', ' + c4.rank_name + ', ' + c5.rank_name */ -- xAAxx
			WHEN c3.pos = c4.pos THEN 'Pair of ' + c3.rank_name_plural /*+ ' with ' + c1.rank_name + ', ' + c2.rank_name + ', ' + c5.rank_name */ -- xxAAx
			WHEN c4.pos = c5.pos THEN 'Pair of ' + c4.rank_name_plural /*+ ' with ' + c1.rank_name + ', ' + c2.rank_name + ', ' + c3.rank_name */ -- xxxAA
			WHEN c1.suit_code = c2.suit_code AND c2.suit_code = c3.suit_code AND c3.suit_code = c4.suit_code AND c4.suit_code = c5.suit_code THEN
				-- All flushes, whether straight flushes or normal
				CASE
					WHEN c1.pos = 14 AND c1.pos = c2.pos + 1 AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 'Royal Flush'
					WHEN c1.pos = c2.pos + 1                 AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN c1.rank_name + '-High Straight Flush'
					WHEN c1.pos = 14 AND c2.pos = 5          AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 'Five-High Straight Flush'
					ELSE c1.rank_name + '-High Flush'
				END
			ELSE
				-- All remaining hands, either straight or high
				CASE
					WHEN c1.pos = c2.pos + 1                 AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN c1.rank_name + '-High Straight'
					WHEN c1.pos = 14 AND c2.pos = 5          AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 'Five-High Straight'
					ELSE c1.rank_name + ' High'
				END
		END 
			AS hand_name, 
		CASE
			-- Assign relative values to the hands (the actual values have no particular meaning other than to enable sorting).
			-- Note that each value is in the form 'abbccddeeff', where
			-- 'a' is a hand type, i.e. 8 = straight flush, 7 = four of a kind, 6 = full house, 5 = flush, 4 = straight, 3 = 3 of a kind, 2 = two pair, 1 = pair, 0 = high
			-- 'bb' is a two-digit representation of the most-significant card(s) in the hand (e.g. a '2' is 02, a '9' is 09, a King is 13, and an Ace is 14)
			-- 'cc' is a two-digit representation of the next most-significant card(s) in the hand
			-- etc.
			-- NOTE (14 Jan 2021): This exactly reproduces the table in PokerHandRankingTable

			-- Four of a kind (AAAAx or xAAAA) [note that I go down to two levels of ranking to defensively allow for a pathological Hold-Em case where a four-of-a-kind is visible in the community cards]
			WHEN c1.pos = c2.pos AND c2.pos = c3.pos AND c3.pos = c4.pos THEN 70000000000 + ( c1.pos * 100000000 ) + ( c5.pos * 1000000 ) -- 4 of a kind - AAAAx
			WHEN c2.pos = c3.pos AND c3.pos = c4.pos AND c4.pos = c5.pos THEN 70000000000 + ( c2.pos * 100000000 ) + ( c1.pos * 1000000 ) -- 4 of a kind - xAAAA
			-- Full house (AAABB or BBAAA)
			WHEN c1.pos = c2.pos AND c2.pos = c3.pos AND c4.pos = c5.pos THEN 60000000000 + ( c1.pos * 100000000 ) + ( c4.pos * 1000000 ) -- full house - AAABB
			WHEN c1.pos = c2.pos AND c3.pos = c4.pos AND c4.pos = c5.pos THEN 60000000000 + ( c3.pos * 100000000 ) + ( c1.pos * 1000000 ) -- full house - BBAAA
			-- Three of a kind (AAAxx or xAAAx or xxAAA)
			WHEN c1.pos = c2.pos AND c2.pos = c3.pos THEN 30000000000 + ( c1.pos * 100000000 ) + ( c4.pos * 1000000 ) + ( c5.pos * 10000 ) -- Three of a kind - AAAxx
			WHEN c2.pos = c3.pos AND c3.pos = c4.pos THEN 30000000000 + ( c2.pos * 100000000 ) + ( c1.pos * 1000000 ) + ( c5.pos * 10000 ) -- Three of a kind - xAAAx
			WHEN c3.pos = c4.pos AND c4.pos = c5.pos THEN 30000000000 + ( c3.pos * 100000000 ) + ( c1.pos * 1000000 ) + ( c2.pos * 10000 ) -- Three of a kind - xxAAA
			-- Two pair (AABBx or AAxBB or xAABB)
			WHEN c1.pos = c2.pos AND c3.pos = c4.pos THEN 20000000000 + ( c1.pos * 100000000 ) + ( c3.pos * 1000000 ) + ( c5.pos * 10000 ) -- Two pair - AABBx
			WHEN c1.pos = c2.pos AND c4.pos = c5.pos THEN 20000000000 + ( c1.pos * 100000000 ) + ( c4.pos * 1000000 ) + ( c3.pos * 10000 ) -- Two pair - AAxBB
			WHEN c2.pos = c3.pos AND c4.pos = c5.pos THEN 20000000000 + ( c2.pos * 100000000 ) + ( c4.pos * 1000000 ) + ( c1.pos * 10000 ) -- Two pair - xAABB
			-- Pair (AAxxx or xAAxx or xxAAx or xxxAA)
			WHEN c1.pos = c2.pos THEN 10000000000 + ( c1.pos * 100000000 ) + ( c3.pos * 1000000 ) + ( c4.pos * 10000 ) + ( c5.pos * 100 ) -- Pair - AAxxx
			WHEN c2.pos = c3.pos THEN 10000000000 + ( c2.pos * 100000000 ) + ( c1.pos * 1000000 ) + ( c4.pos * 10000 ) + ( c5.pos * 100 ) -- Pair - xAAxx
			WHEN c3.pos = c4.pos THEN 10000000000 + ( c3.pos * 100000000 ) + ( c1.pos * 1000000 ) + ( c2.pos * 10000 ) + ( c5.pos * 100 ) -- Pair - xxAAx
			WHEN c4.pos = c5.pos THEN 10000000000 + ( c4.pos * 100000000 ) + ( c1.pos * 1000000 ) + ( c2.pos * 10000 ) + ( c3.pos * 100 ) -- Pair - xxxAA
			WHEN c1.suit_code = c2.suit_code AND c2.suit_code = c3.suit_code AND c3.suit_code = c4.suit_code AND c4.suit_code = c5.suit_code THEN
				-- All flushes, whether straight flushes or normal
				CASE
					WHEN c1.pos = 14 AND c1.pos = c2.pos + 1 AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 80000000000 + ( c1.pos * 100000000 ) -- Royal Flush
					WHEN c1.pos = c2.pos + 1                 AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 80000000000 + ( c1.pos * 100000000 ) -- Other Straight Flush
					WHEN c1.pos = 14 AND c2.pos = 5          AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 80000000000 + ( c2.pos * 100000000 ) -- Five High Straight Flush
					ELSE 50000000000 + ( c1.pos * 100000000 ) + ( c2.pos * 1000000 ) + ( c3.pos * 10000 ) + ( c4.pos * 100 ) + c5.pos  -- x-High Flush'
				END
			ELSE
				-- All remaining hands, either straight or high
				CASE
					WHEN c1.pos = c2.pos + 1                 AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 40000000000 + ( c1.pos * 100000000 ) -- Most straights
					WHEN c1.pos = 14 AND c2.pos = 5          AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 40000000000 + ( c2.pos * 100000000 ) -- Wheel (A-2-3-4-5)
					ELSE 00000000000 + ( c1.pos * 100000000 ) + ( c2.pos * 1000000 ) + ( c3.pos * 10000 ) + ( c4.pos * 100 ) + c5.pos  -- High
				END
		END 
			AS hand_value,
		CASE
			-- Show how the hand would be best presented if you sorted the player's card in descending order left-to-right and then re-ordered them.
			-- E.g. if a player has 'A3737' as a hand, we would want to present this as '7733A' i.e. the highest pair, then the second pair then the 'kicker'
			-- Finding the correct presentation sequence involves a number of steps:
			-- (1) Sort the player's cards in descending order left-to-right (giving A7733 in the example) and note which original card position filled each slot, '13524' in the example
			-- (2) Lookup the reference order for the hand from the entries below, e.g. '23451' in the case of xAABB below (where '23451' means 'for position 1 use card 2 of the sorted hand, for pos 2 used card 3, etc.')
			-- (3) For the first position, take the '2' from the first position of '23451', then use the original position mapping to find the card to use,
			--     e.g. position 2 of '13524' is '3', so use the 3rd card of the original hand 'A3737', i.e. the first 7, to fill position 1
			--     Repeat for position 2: position 2 of the reference order contains a '3', and position 3 of the position mapping shows a 5, so take the 5th card of the original hand, i.e. the second 7, to fill position 2
			--     Repeat for position 3: position 3 of the reference order contains a '4', and position 4 of the position mapping shows a 2, so take the 2nd card of the original hand, i.e. the first 3,  to fill position 3
			--     Repeat for position 4: position 4 of the reference order contains a '5', and position 5 of the position mapping shows a 4, so take the 4th card of the original hand, i.e. the second 3,  to fill position 4
			--     Repeat for position 5: position 5 of the reference order contains a '1', and position 1 of the position mapping shows a 1, so take the 1st card of the original hand, i.e. the Ace,  to fill position 5
			WHEN c1.pos = c2.pos AND c2.pos = c3.pos AND c3.pos = c4.pos THEN 12345 -- 4 of a kind - AAAAx
			WHEN c2.pos = c3.pos AND c3.pos = c4.pos AND c4.pos = c5.pos THEN 23451 -- 4 of a kind - xAAAA
			-- Full house (AAABB or BBAAA)
			WHEN c1.pos = c2.pos AND c2.pos = c3.pos AND c4.pos = c5.pos THEN 12345 -- full house - AAABB
			WHEN c1.pos = c2.pos AND c3.pos = c4.pos AND c4.pos = c5.pos THEN 34512 -- full house - BBAAA
			-- Three of a kind (AAAxx or xAAAx or xxAAA)
			WHEN c1.pos = c2.pos AND c2.pos = c3.pos THEN 12345 -- Three of a kind - AAAxx
			WHEN c2.pos = c3.pos AND c3.pos = c4.pos THEN 23415 -- Three of a kind - xAAAx
			WHEN c3.pos = c4.pos AND c4.pos = c5.pos THEN 34512 -- Three of a kind - xxAAA
			-- Two pair (AABBx or AAxBB or xAABB)
			WHEN c1.pos = c2.pos AND c3.pos = c4.pos THEN 12345 -- Two pair - AABBx
			WHEN c1.pos = c2.pos AND c4.pos = c5.pos THEN 12453 -- Two pair - AAxBB
			WHEN c2.pos = c3.pos AND c4.pos = c5.pos THEN 23451 -- Two pair - xAABB
			-- Pair (AAxxx or xAAxx or xxAAx or xxxAA)
			WHEN c1.pos = c2.pos THEN 12345 -- Pair - AAxxx
			WHEN c2.pos = c3.pos THEN 23145 -- Pair - xAAxx
			WHEN c3.pos = c4.pos THEN 34125 -- Pair - xxAAx
			WHEN c4.pos = c5.pos THEN 45123 -- Pair - xxxAA
			WHEN c1.suit_code = c2.suit_code AND c2.suit_code = c3.suit_code AND c3.suit_code = c4.suit_code AND c4.suit_code = c5.suit_code THEN
				-- All flushes, whether straight flushes or normal
				CASE
					WHEN c1.pos = 14 AND c1.pos = c2.pos + 1 AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 54321  -- Royal Flush
					WHEN c1.pos = c2.pos + 1                 AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 54321 -- Other Straight Flush
					WHEN c1.pos = 14 AND c2.pos = 5          AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 15432 -- Five High Straight Flush
					ELSE                                                                                                                  12345 -- x-High Flush'
				END
			ELSE
				-- All remaining hands, either straight or high
				CASE
					WHEN c1.pos = c2.pos + 1                 AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 54321 -- Most straights
					WHEN c1.pos = 14 AND c2.pos = 5          AND c2.pos = c3.pos + 1 AND c3.pos = c4.pos + 1 AND c4.pos = c5.pos + 1 THEN 15432 -- Wheel (A-2-3-4-5)
					ELSE                                                                                                                  12345 -- High
				END
		END 
			AS presentation_order
	FROM #cards c1
	CROSS JOIN #cards c2
	CROSS JOIN #cards c3
	CROSS JOIN #cards c4
	CROSS JOIN #cards c5
	WHERE ( 
		c1.card_code <> c2.card_code AND
		c1.card_code <> c3.card_code AND 
		c1.card_code <> c4.card_code AND 
		c1.card_code <> c5.card_code AND 
		c2.card_code <> c3.card_code AND
		c2.card_code <> c4.card_code AND
		c2.card_code <> c5.card_code AND
		c3.card_code <> c4.card_code AND
		c3.card_code <> c5.card_code AND
		c4.card_code <> c5.card_code 
	) -- don't allow a card to be combined with itself
	AND ( 
		c1.pos >= c2.pos AND
		c2.pos >= c3.pos AND
		c3.pos >= c4.pos AND 
		c4.pos >= c5.pos 
	) -- consider only the combinations where the cards are in order from left to right
) d

/*
SELECT * FROM #combos5 ORDER BY hand_value DESC -- High hand values is better hand 
SELECT * FROM #combos5 ORDER BY eval_rank ASC -- Low rank is a better hand
*/

-- Old style, to help check 1:1 match with downloaded code
select 'EvalHands.Add('+LTRIM(STR(hand_signature))
	+', new EvalHand('+LTRIM(STR(hand_signature))	
	+', '+LTRIM(STR(eval_rank))	
	+', "'+hand_name
	+'"));' as new_eval_stmt
from #combos5 
ORDER BY hand_signature ASC

-- New style, add some additional info that SPC requires
select 'EvalHands.Add('+LTRIM(STR(hand_signature))
	+', new EvalHand('+LTRIM(STR(hand_signature))	
	+', '+LTRIM(STR(eval_rank))	
	+', "'+hand_name
	+', "'+LTRIM(STR(presentation_order))
	+', "'+LTRIM(STR(hand_value,11,0))
	+'"));' as new_eval_stmt
from #combos5 
ORDER BY hand_signature ASC

-- EvalHands.Add(-25911877, new EvalHand(-25911877, 323, "Ace-High Flush"));

