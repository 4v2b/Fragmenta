import { useState, useEffect } from "react";
import { Input, Box, List, Spinner } from "@chakra-ui/react";
import { api } from "@/api/fetchClient";

export function Autocomplete({ addItem }) {
  const [query, setQuery] = useState("");
  const [suggestions, setSuggestions] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const delayDebounce = setTimeout(() => {
      if (query.trim()) {
        setLoading(true);
        api.get(`/users/lookup?email=${query}`)
          .then((data) => setSuggestions(data))
          .finally(() => setLoading(false));
      } else {
        setSuggestions([]);
      }
    }, 600);

    return () => clearTimeout(delayDebounce);
  }, [query]);

  return (
    <Box position="relative" w="full">
      <Input
        value={query}
        onChange={(e) => setQuery(e.target.value)}
        placeholder="Search..."
      />
      {loading && <Spinner position="absolute" top="50%" right="10px" size="sm" />}
      {!loading && suggestions.length > 0 && (
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
          {suggestions.map((item) => (
            <List.Item onClick={() =>{ addItem(item); setSuggestions([]); setQuery("")}}
              key={item.id}
              p="2"
              _hover={{ bg: "gray.100", cursor: "pointer" }}
            >
              {item.email}
            </List.Item>
          ))}
        </List.Root>
      )}
    </Box>
  );
}
