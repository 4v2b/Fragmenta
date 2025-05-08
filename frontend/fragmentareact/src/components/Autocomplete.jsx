import { useState, useEffect } from "react";
import { Input, Box, List, Spinner } from "@chakra-ui/react";
import { api } from "@/api/fetchClient";
import { useTranslation } from "react-i18next";

export function Autocomplete({ addItem, membersBlacklist = [] }) {
  const [query, setQuery] = useState("");
  const [suggestions, setSuggestions] = useState([]);
  const [loading, setLoading] = useState(false);
  const {t} = useTranslation();

  useEffect(() => {
    const delayDebounce = setTimeout(() => {
      if (query.trim()) {
        setLoading(true);
        api.get(`/lookup-users?email=${query}`)
          .then((data) => setSuggestions(data.filter(fetchedUser =>
            !membersBlacklist.some(memeber => memeber.email === fetchedUser.email)
          )))
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
        placeholder={t("fields.labels.searchUser")}
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

          {suggestions.length > 0 ? suggestions.map((item) => (
            <List.Item onClick={() => { addItem(item); setSuggestions([]); setQuery("") }}
              key={item.id}
              p="2"
              _hover={{ bg: "gray.100", cursor: "pointer" }}
            >
              {item.email}
            </List.Item>
          )) : <List.Item p="2">{t("errors.userNotFound")}</List.Item>} 
        </List.Root>
      )}
    </Box>
  );
}
