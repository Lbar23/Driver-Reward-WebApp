import React from 'react';
import { Accordion, AccordionSummary, AccordionDetails, Typography } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';

// Interface for link data
interface LinkData {
  text: string;
  href: string;
}

// Interface for FAQ item with optional links
interface FAQItemProps {
  question: string;
  answer: string;
  links?: LinkData[];
}

const StyledLink: React.FC<LinkData> = ({ text, href }) => (
  <a 
    href={href}
    className="inline-block border border-blue-500 rounded px-1 text-blue-600 hover:bg-blue-50"
    style={{
      display: 'inline-block',
      border: '1px solid #3b82f6',
      borderRadius: '4px',
      padding: '0 4px',
      color: '#2563eb',
      textDecoration: 'none'
    }}
    onMouseOver={(e) => {
      e.currentTarget.style.backgroundColor = '#eff6ff';
    }}
    onMouseOut={(e) => {
      e.currentTarget.style.backgroundColor = 'transparent';
    }}
  >
    {text}
  </a>
);

// Function to replace link placeholders with actual link components
const renderAnswerWithLinks = (answer: string, links?: LinkData[]) => {
  if (!links || links.length === 0) return answer;

  let result = answer;
  links.forEach(link => {
    result = result.replace(
      link.text,
      `__LINK_${link.text}__`
    );
  });

  const parts = result.split(/__LINK_|__/);
  return parts.map((part, index) => {
    const link = links.find(l => l.text === part);
    return link ? (
      <StyledLink key={index} text={link.text} href={link.href} />
    ) : (
      <span key={index}>{part}</span>
    );
  });
};

const FAQItem: React.FC<FAQItemProps> = ({ question, answer, links }) => (
  <Accordion>
    <AccordionSummary
      expandIcon={<ExpandMoreIcon />}
      aria-controls="panel-content"
      id="panel-header"
    >
      <Typography>{question}</Typography>
    </AccordionSummary>
    <AccordionDetails>
      <Typography component="div">
        {renderAnswerWithLinks(answer, links)}
      </Typography>
    </AccordionDetails>
  </Accordion>
);

interface FAQData extends FAQItemProps {}

const FAQ: React.FC = () => {
  const faqs: FAQData[] = [
    {
      question: "How do I create an account?",
      answer: "To create an account, click on the 'Sign Up' Button, and from there you will become a guest.",
      links: [
        { text: "Sign Up", href: "/register" }
      ]
    },
    {
      question: "How can I become a driver?",
      answer: "To become a driver, you need to apply with one of the sponsors associated with our program. Once they have accepted your application, you will automatically become a driver."
    },
    {
      question: "What are the password requirements?",
      answer: "To create a password, it must have a number, a lowercase letter, an uppercase letter, be at least 8 characters long and no characters can be repeated."
    },
    {
      question: "How can I reset my password?",
      answer: "If you've forgotten your password, click on the 'Forgot Password' link on the login page.",
      links: [
        { text: "Forgot Password", href: "/reset-password" },
        { text: "login page", href: "/login" }
      ]
    },
    {
      question: "How do I earn points?",
      answer: "To earn points, first you must be associated with at least one sponsor. From there, simply follow instructions from your sponsor to earn points."
    },
    {
      question: "How do I apply to a sponsor?",
      answer: "To apply to be a sponsor, simply reach out to us and we will create an account for you if you meet our requirements."
    },
    {
      question: "What can I spend my points on?",
      answer: "You can exchange your points for a variety of items from your sponsors catalog. Each of our sponsors has a unique selection of items.",
      links: [
        { text: "sponsors catalog", href: "/catalog" }
      ]
    }
  ];

  return (
    <div>
      <Typography variant="h4" gutterBottom>Frequently Asked Questions</Typography>
      {faqs.map((faq, index) => (
        <FAQItem 
          key={index} 
          question={faq.question} 
          answer={faq.answer}
          links={faq.links}
        />
      ))}
    </div>
  );
};

export default FAQ;