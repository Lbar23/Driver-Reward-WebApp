import React, { createContext, useContext, useRef, useEffect } from 'react';

interface AccessibilityContextType {
  announce: (message: string, priority?: 'polite' | 'assertive') => void;
  focusElement: (selector: string) => void;
}

const AccessibilityContext = createContext<AccessibilityContextType | null>(null);

export const AccessibilityProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const announcer = useRef<HTMLDivElement>(null);

  // Screen reader announcements
  const announce = (message: string, priority: 'polite' | 'assertive' = 'polite') => {
    if (!announcer.current) return;
    
    announcer.current.setAttribute('aria-live', priority);
    announcer.current.textContent = message;
    
    // Clear after the announcement ends
    setTimeout(() => {
      if (announcer.current) announcer.current.textContent = '';
    }, 1000);
  };

  //Focus management
  const focusElement = (selector: string) => {
    const element = document.querySelector<HTMLElement>(selector);
    if (element) {
      element.focus();
      announce(`Focused ${element.getAttribute('aria-label') || 'element'}`);
    }
  };

  // Global keyboard navigation
  //Add more to these later; Basic rendition
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Common keyboard shortcuts
      if (e.altKey) {
        switch(e.key) {
          case 'h':
            e.preventDefault();
            focusElement('#dashboard-home');
            break;
          case 'm':
            e.preventDefault();
            focusElement('#main-content');
            break;
            // These below are mainly for Sponsors and Admins when viewing role changes
          case 'n':
            e.preventDefault();
            focusElement('[role="navigation"]');
            break;
          case 's':
            e.preventDefault();
            focusElement('[role="search"]');
            break;
        }
      }

      //Escape key handling
      if (e.key === 'Escape') {
        // Close any open modals or menus
        document.querySelector<HTMLElement>('[role="dialog"] [aria-label="Close"]')?.click();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  return (
    <AccessibilityContext.Provider value={{ announce, focusElement }}>
      {/* Screen reader announcer */}
      <div
        ref={announcer}
        aria-live="polite"
        aria-atomic="true"
        style={{
          position: 'absolute',
          width: '1px',
          height: '1px',
          padding: '0',
          margin: '-1px',
          overflow: 'hidden',
          clip: 'rect(0, 0, 0, 0)',
          whiteSpace: 'nowrap',
          border: '0'
        }}
      />
      {children}
    </AccessibilityContext.Provider>
  );
};

export const useAccessibility = () => {
  const context = useContext(AccessibilityContext);
  if (!context) {
    throw new Error('useAccessibility must be used within an AccessibilityProvider');
  }
  return context;
};