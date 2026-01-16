/**
 * Unit tests for StoryRootEditor component
 * 
 * Note: These tests require test infrastructure setup (Jest, React Testing Library)
 * Run: npm test -- StoryRootEditor.test.tsx
 */

import { render, screen, fireEvent } from '@testing-library/react';
import { StoryRootEditor } from '../StoryRootEditor';
import { StoryRoot } from '../../../models';

describe('StoryRootEditor', () => {
    const mockStoryRoot: StoryRoot = {
        story_root_id: 'test-id',
        genre: 'Fantasy',
        tone: 'Epic',
        thematic_pillars: 'Courage, Honor, Sacrifice',
        notes: 'Optional notes',
    };

    it('should render editable Story Root form', () => {
        const onChange = jest.fn();
        render(<StoryRootEditor storyRoot={mockStoryRoot} onChange={onChange} />);
        
        expect(screen.getByText('Edit Story Root')).toBeInTheDocument();
        expect(screen.getByDisplayValue('Fantasy')).toBeInTheDocument();
        expect(screen.getByDisplayValue('Epic')).toBeInTheDocument();
    });

    it('should call onChange when field is edited', () => {
        const onChange = jest.fn();
        render(<StoryRootEditor storyRoot={mockStoryRoot} onChange={onChange} />);
        
        const genreField = screen.getByLabelText('Genre');
        fireEvent.change(genreField, { target: { value: 'Sci-Fi' } });
        
        expect(onChange).toHaveBeenCalled();
    });

    it('should update local state when storyRoot prop changes', () => {
        const onChange = jest.fn();
        const { rerender } = render(<StoryRootEditor storyRoot={mockStoryRoot} onChange={onChange} />);
        
        const updatedStoryRoot: StoryRoot = {
            ...mockStoryRoot,
            genre: 'Updated Genre',
        };
        
        rerender(<StoryRootEditor storyRoot={updatedStoryRoot} onChange={onChange} />);
        
        expect(screen.getByDisplayValue('Updated Genre')).toBeInTheDocument();
    });

    it('should handle optional notes field', () => {
        const storyRootWithoutNotes: StoryRoot = {
            ...mockStoryRoot,
            notes: undefined,
        };
        
        const onChange = jest.fn();
        render(<StoryRootEditor storyRoot={storyRootWithoutNotes} onChange={onChange} />);
        
        const notesField = screen.getByLabelText('Notes');
        expect(notesField).toBeInTheDocument();
        expect(notesField).toHaveValue('');
    });
});
