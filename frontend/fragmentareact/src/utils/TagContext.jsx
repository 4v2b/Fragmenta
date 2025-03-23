import { api } from '@/api/fetchClient';
import { createContext, useState, useContext, useEffect } from 'react';
import { useParams } from 'react-router';

const TagsContext = createContext();

export function TagsProvider({ children }) {
  const [tags, setTags] = useState([]);
  const { workspaceId, boardId } = useParams()

  useEffect(() => {
    api.get(`/tags?boardId=${boardId}`, workspaceId).then(res => setTags(res));
  }, [boardId]);

  function addTag(newTagName) {
    return api.post(`/tags?name=${newTagName}&boardId=${boardId}`, {}, workspaceId)
      .then(res => {
        if (res) {
          setTags([...tags, res]);
          return res; // Return the created tag
        }
      });
  }

  function removeTag(tagId) {
    api.delete(`/tags/${tagId}`, workspaceId).then(() => {
      setTags(tags.filter(tag => tag.id !== tagId));
    })
  };

  return (
    <TagsContext.Provider value={{ tags, addTag, removeTag }}>
      {children}
    </TagsContext.Provider>
  );
}

export function useTags() {
  return useContext(TagsContext);
}