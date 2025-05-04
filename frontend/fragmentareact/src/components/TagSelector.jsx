import { useState, useEffect } from "react";
import { Input, Tag, Box, Flex, List } from "@chakra-ui/react";
import { useTags } from "@/utils/TagContext"

export function TagSelector({ selectedTags, onSelect, onRemove }) {
    const [suggestedTags, setSuggestedTags] = useState([])
    const [query, setQuery] = useState("");
    const [showSuggestions, setShowSuggestions] = useState(false)
    const { tags, addTag } = useTags();

    function handleSelect(item) {
        onSelect(item);
        setSuggestedTags([]);
        setQuery("");
        setShowSuggestions(false);
    }

    function handleRemove(item) {
        console.log("removed", item);
        onRemove(item);
        // setSuggestedTags([]);
        // setQuery("");
        // setShowSuggestions(false);
    }

    console.log(selectedTags)

    useEffect(() => {
        const delayDebounce = setTimeout(() => {
            setSuggestedTags(tags.filter(e => e.name?.includes(query) && !selectedTags?.some(i => i.id == e.id)))
        }, 200);

        return () => clearTimeout(delayDebounce);
    }, [query]);

    return (

        <Box>
            <Box position="relative" w="full">
                <Input
                    className="input-tag"
                    onBlur={() => setTimeout(() => setShowSuggestions(false), 100)}
                    value={query}
                    onFocus={() => setShowSuggestions(true)}
                    onChange={(e) => setQuery(e.target.value)}
                    placeholder="Search..."
                />

                {(showSuggestions) && (
                    <List.Root
                        variant="plain"
                        position="absolute"
                        top="100%"
                        left="0"
                        width="100%"
                        bg="white"
                        border="1px solid"
                        borderColor="gray.200"
                        borderRadius="md"
                        boxShadow="md"
                        zIndex="10"
                    >
                        {query === "" ? (
                            // When query is empty, show all available tags
                            tags.length > 0 ?
                                tags.filter(tag => !selectedTags?.some(i => i.id === tag.id))
                                    .map(item => (
                                        <List.Item
                                        
                                            key={item.id}
                                            onMouseDown={(e) => {
                                                e.preventDefault(); // Prevent input from losing focus immediately
                                                handleSelect(item);
                                            }}
                                            p="2"
                                            _hover={{ bg: "gray.100", cursor: "pointer" }}
                                        >{item.name}</List.Item>
                                    ))
                                : <List.Item>{"No tags available"}</List.Item>
                        ) : (
                            // When query has text
                            suggestedTags.length > 0 ?
                                suggestedTags.map(item => (
                                    <List.Item
                                        className="suggested-tag"
                                        onMouseDown={(e) => {
                                            e.preventDefault(); // Prevent input from losing focus immediately
                                            handleSelect(item);
                                        }}
                                        p="2"
                                        _hover={{ bg: "gray.100", cursor: "pointer" }}
                                        key={item.id}>{item.name}</List.Item>
                                ))
                                :
                                <>
                                    <List.Item
                                        p="2"
                                        fontStyle={"italic"}
                                    >{"No tags found"}</List.Item>
                                    <List.Item
                                        className="create-tag"
                                        fontWeight="semibold"
                                        onMouseDown={(e) => {
                                            e.preventDefault(); // Prevent input from losing focus immediately
                                            addTag(query).then(newTag => {
                                                if (newTag) {
                                                    handleSelect(newTag); // Automatically select the newly created tag
                                                }
                                            });
                                        }}
                                        p="2"
                                        _hover={{ bg: "gray.100", cursor: "pointer" }}
                                    >{`Create tag "${query}"`}</List.Item>
                                </>
                        )}
                    </List.Root>
                )}
            </Box>
            <Flex gap="4" wrap="wrap">
                {selectedTags.map(e => <Tag.Root className="tag-root" key={e.id} size="md" colorPalette="white">
                    <Tag.Label className="tag-label" >{e.name}</Tag.Label>
                    <Tag.EndElement>
                        <Tag.CloseTrigger className="remove-tag" onClick={() => onRemove(e)} />
                    </Tag.EndElement>
                </Tag.Root>)}
            </Flex>
        </Box>
    );
}