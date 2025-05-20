import { useState, useEffect } from "react";
import { Input, Box, List } from "@chakra-ui/react";
import { useTranslation } from "react-i18next";

export function MemberSelector({ members, onSelect }) {
    const [selectedMembers, setSelectedMembers] = useState([])
    const [query, setQuery] = useState("");
    const [showSuggestions, setShowSuggestions] = useState(false)
const {t} = useTranslation()
    function handleSelect(item) {
        onSelect(item);
        setSelectedMembers([]);
        setQuery("")
    }

    useEffect(() => {
        const delayDebounce = setTimeout(() => {
            setSelectedMembers(members.filter(e => e.email.includes(query) || e.name.includes(query)))
        }, 200);

        return () => clearTimeout(delayDebounce);
    }, [query]);

    return (
        <Box position="relative" w="full">
            <Input
            className="select-member"
                onBlur={() => setTimeout(() => setShowSuggestions(false), 100)}
                value={query}
                onFocus={() => setShowSuggestions(true)}
                onChange={(e) => setQuery(e.target.value)}
                placeholder={t("common.search")}
            />
            {(selectedMembers.length > 0 && showSuggestions) && (
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
                    {selectedMembers.map((item) => (
                        <List.Item
                            className="suggested-member"
                            onMouseDown={(e) => {
                                e.preventDefault(); 
                                handleSelect(item);
                            }} key={item.id}
                            p="2"
                            _hover={{ bg: "gray.100", cursor: "pointer" }}
                        >
                            {item.name}
                        </List.Item>
                    ))}
                </List.Root>
            )}
        </Box>
    );
}